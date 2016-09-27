using System;
using System.Collections.Generic;
using System.IO;

//Helper classes to MenuCanvasControl.cs


//class to store menu sceneEntries
public class SceneEntry
{
    public string varName { get; set; }
    public string val { get; set; }
    public float time { get; set; }

    public SceneEntry(string _varName, string _val, float entryTime)
    {
        varName = _varName;
        val = _val;
        time = entryTime;
    }


}
//class to manage and to store and load list of sceneEntries
public class Scene
{ 
    private List<SceneEntry> _sceneEntries;



    public Scene()
    {
        _sceneEntries = new List<SceneEntry>();
    }

    public int Count
    {
        get { return _sceneEntries.Count; }
    }

    public List<SceneEntry> sceneEntries
    {
        get { return _sceneEntries; }
    }

    public void Add(SceneEntry newEntry)
    {
        //search for existing entry with the same level and same variable name
        if (_sceneEntries.Count < 1)
        {
            _sceneEntries.Add(newEntry);
            return;
        }
        bool bFoundName = false;
        foreach (SceneEntry entry in _sceneEntries)
        {
            if (entry.varName.Equals(newEntry.varName))
            {
                bFoundName = true;
                if (!entry.val.Equals(newEntry.val))
                {
                    entry.val = newEntry.val;
                    entry.time = newEntry.time;

                    break;
                }
            }           
        }
        if (!bFoundName)
        {
           _sceneEntries.Add(newEntry);
        }
    }

    public SceneEntry GetEntry(string myVarName)
    {
        foreach (SceneEntry entry in _sceneEntries)
        {
            if (entry.varName.Equals(myVarName))
                return entry;
        }
        return null;
    }
    public string GetEntryVal(string myVarName)
    {
        foreach (SceneEntry entry in _sceneEntries)
        {
            if (entry.varName.Equals(myVarName))
                return entry.val;
         }
        return "";
    }

    //did not fly (collection was proteced)
    public string GetAndRemoveEntryVal(string myVarName)
    {
        string strVal = "";
        foreach (SceneEntry entry in _sceneEntries)
        {
            if (entry.varName.Equals(myVarName))
            {
                strVal = entry.val;
                _sceneEntries.Remove(entry);
                return strVal;
            }
           
        }
        return strVal;
    }
    /*****************************************
        //will not be used when using randomized tasks containing one to many scenes
        public void StoreScene()
        {
    #if UNITY_STANDALONE_OSX
            string path = System.Environment.GetFolderPath (System.Environment.SpecialFolder.MyDocuments) + @"/" + Application.productName;
            if (!Directory.Exists (path))
                Directory.CreateDirectory (path);
            string menuFilename = path + @"/" + "SphereMenu" + DateTime.Now.ToString ("yyyyMMdd_HHmmss") + ".txt";
    #else
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + @"\" + Application.productName;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string menuFilename = path + @"\SphereMenu" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
    #endif
            Debug.Log("Filename: " + menuFilename);
            string newContent = "";
            foreach (SceneEntry entry in _sceneEntries)
            {
                newContent += (entry.varName + "," + entry.val + "," + entry.time.ToString() + Environment.NewLine);
            }
            File.AppendAllText(menuFilename, newContent);
            //_scene.Clear ();
        }
        // menuFilename shoudl contain the full path name
        // will not be used for paradigm with tasks containing one to many scnes (i.e. sceneEntries).
        public void LoadScene(string menuFilename)
        {
            if (File.Exists(menuFilename))
            {
                StreamReader fs = File.OpenText(menuFilename);
                string content = fs.ReadLine();
                char[] separators = { ',', ' ' };
                int level;
                //clear sceneEntries list
                _sceneEntries.Clear();
                for (;;)
                {
                    if (fs.EndOfStream)
                        break;
                    content = fs.ReadLine();
                    string[] parts = content.Split(separators, StringSplitOptions.None);
                    bool success = int.TryParse(parts[0], out level);
                    if (success)
                    {
                        SceneEntry item = new SceneEntry(parts[1], parts[2], 0f);
                        Add(item);
                    }
                }
            }
            ********************************/
}

