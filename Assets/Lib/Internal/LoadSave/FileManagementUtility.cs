using System;
using System.IO;

static public class FileManagementUtility
{
    /// <summary>
    /// Create a folder
    /// </summary>
    /// <param name="pathFromDoc">the entire path from the parent to the directory itself
    /// <para>In this format \a\b\c\x</para></param>
    static public void CreateGameFolder(string pathFromDoc)
    {
        try
        {
            if (Directory.Exists(pathFromDoc))
            {
                //Debug.Log(pathFromDoc + " Already Exists");
                return;
            }

            Directory.CreateDirectory(pathFromDoc);
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR: " + e.ToString());
        }
    }

    /// <summary>
    /// Checks for a folder
    /// </summary>
    /// <param name="pathFromDoc">the entire path from the parent to the directory itself
    /// <para>In this format \a\b\c\x</para></param>
    static public bool CheckExists(string pathFromDoc)
    {
        try
        {
            //Debug.Log("path = " + path);
            if (Directory.Exists(pathFromDoc))
            {
                //Debug.Log("Exists");				
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR: " + e.ToString());
            return false;
        }
    }
}
