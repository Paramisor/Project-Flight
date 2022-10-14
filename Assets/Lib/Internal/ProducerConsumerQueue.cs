using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class ProducerConsumerQueue : IDisposable
{
    class WorkItem
    {
        public readonly TaskCompletionSource<object> TaskSource;
        public readonly Action Action;
        public readonly CancellationToken? CancelToken;

        public WorkItem(
          TaskCompletionSource<object> taskSource,
          Action action,
          CancellationToken? cancelToken)
        {
            TaskSource = taskSource;
            Action = action;
            CancelToken = cancelToken;
        }
    }

    BlockingCollection<WorkItem> _taskQ = new BlockingCollection<WorkItem>();
    public CancellationTokenSource cancelSource = new CancellationTokenSource();

    public int currentWorkerCount
    {
        get; private set;
    }

    public ProducerConsumerQueue(int workerCount)
    {
        currentWorkerCount = workerCount;
        // Create and start a separate Task for each consumer:
        for (int i = 0; i < workerCount; i++)
            Task.Factory.StartNew(Consume);
    }

    public void Dispose() { _taskQ.CompleteAdding(); }

    public Task EnqueueTask(Action action)
    {
        return EnqueueTask(action, null);
    }

    public Task EnqueueTask(Action action, CancellationToken? cancelToken)
    {
        var tcs = new TaskCompletionSource<object>();
        _taskQ.Add(new WorkItem(tcs, action, cancelToken));
        return tcs.Task;
    }

    void Consume()
    {
        foreach (WorkItem workItem in _taskQ.GetConsumingEnumerable())
            if (workItem.CancelToken.HasValue &&
                workItem.CancelToken.Value.IsCancellationRequested)
            {
                workItem.TaskSource.SetCanceled();
            }
            else
                try
                {
                    workItem.Action();
                    workItem.TaskSource.SetResult(null);   // Indicate completion
                }
                catch (OperationCanceledException ex)
                {
                    if (ex.CancellationToken == workItem.CancelToken)
                        workItem.TaskSource.SetCanceled();
                    else
                        workItem.TaskSource.SetException(ex);
                }
                catch (Exception ex)
                {
                    workItem.TaskSource.SetException(ex);
                }
    }
}