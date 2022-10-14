using UnityEngine;

public class LoadCoordinator : MonoBehaviour
{
    RectTransform LoadBar;
    float width;
    // Start is called before the first frame update
    void Start()
    {
        LoadBar = this.transform.Find("Scrollbar/Sliding Area/Handle").GetComponent<RectTransform>();
        width = LoadBar.transform.parent.GetComponent<RectTransform>().sizeDelta.x;
    }

    // Update is called once per frame
    void Update()
    {
        LoadBar.SetSizeWithCurrentAnchors(0, (FileImportHandler.LoadStatus / 100) * width);
        if (FileImportHandler.LoadStatus == 100)
        { this.gameObject.SetActive(false); }
    }
}
