using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


// 2 helper classes to MenuCanvasControl to manage, store and loads tasks using helper classes in Entries.cs

public class Task
{
    private List<Scene> _scenes;
    private int _sceneIndex;

    public List<Scene> scenes
    {
        get
        {
            return _scenes;
        }
    }
    public int sceneIndex
    {
        get { return _sceneIndex; }
    }
    public Task()
    {
        _scenes = new List<Scene>();
        _sceneIndex = -1;
        Scene scene = new Scene();
        AddScene(scene);
    }

    public void AddScene(Scene scene)
    {
        _scenes.Add(scene);
        _sceneIndex++;
    }
    public Scene GetScene()
    {
        if (_sceneIndex < 0)
            return null;
        else
            return _scenes[_sceneIndex];
    }
          
    public Scene GetScene(int index)
    {
        if ((index >= 0) && (index < _scenes.Count))
            return _scenes[index];
        else
            return null;
    }

    public Scene NextScene()
    {
        if (_sceneIndex < (_scenes.Count - 1))
        {
            _sceneIndex++;
            return _scenes[_sceneIndex];
        }
        else
        {
            return null;
        }
    }

    public void UpdateScene (Scene scene)
    {
        _scenes[_sceneIndex] = scene;
    }

    public void UpdateScene(Scene scene, int index)
    {
        if ((index >= 0) && (index < _scenes.Count))
            _scenes[index] = scene;
    }
    public void ResetSceneIndex()
    {
        _sceneIndex = 0;
    }
}

public class TaskList
{
    //Basic memebers
    private List<Task> _tasks;
    private List<int> _randomIndex;
    private int _taskIndex;
    public int maxTaskNum { get; set; }

    //control switches
    public bool bRandomize { get; set; }
    public bool bEditing { get; set; }

    //reference menu lists
    private Scene refMenu; //content of starting menu
    private Scene add2RefMenu; //changes made to starting menu

    public int Count
    {
        get { return _tasks.Count; }
    }

    public List<Task> tasks
    {
        get { return _tasks; }
    }

    /********************
    public TaskList()
    {
        _tasks = new List<Task>();
        _randomIndex = new List<int>();
        _taskIndex = -1;
        bEditing = true;
        bRandomize = false;
        maxTaskNum = 10;

        refMenu = null;
        add2RefMenu = null; 

        Task task = new Task();
         AddTask(task);
    }
    ************************/

    public TaskList(int maxTasks = 10, bool isRandomized = false)
    {
        _tasks = new List<Task>();
        _randomIndex = new List<int>();
        _taskIndex = -1;
        bEditing = true;
        bRandomize = isRandomized;
        maxTaskNum = maxTasks;


        refMenu = null;
        add2RefMenu = null;

        Task task = new Task();
        AddTask(task);
    }


    //Randomize after adding the last task/ do not randomize the first starting task
    private void RandomizeTasks()
    {
        System.Random rng = new System.Random();
        int val;

        int n = _randomIndex.Count;
        for (int i = 0; i < n; i++)
            _randomIndex[i] = i;
        //we do not want to randomize the first member in the list, which should be always zero (the starting condition/end condition)
        //aus dem Internat geklauter algorithmus (shuffleList<T>())
        while (n > 2)
        {
            n--;
            int k = rng.Next(1, n + 1);
            val = _randomIndex[k];
            _randomIndex[k] = _randomIndex[n];
            _randomIndex[n] = val;
        }
    }

    private void SortTasks()
    {
         for (int i = 0; i < _randomIndex.Count; i++)
            _randomIndex[i] = i;
    }

    public void AddTask()
    {
        Task task = new Task();
        AddTask(task);
    }

    public void AddTask(Task task)
    {
        _tasks.Add(task);
        _taskIndex++;
        _randomIndex.Add(_taskIndex);
    }
  
    public int taskIndex
    {
        get { return _taskIndex; }
    }
    public int taskId
    {
        get { return _randomIndex[_taskIndex]; }
    }

    public Task GetTask()
    {

        if ((_taskIndex >= 0) && (_taskIndex < _tasks.Count))
        {
            int i = _randomIndex[_taskIndex];
            Task task = _tasks[i];
             return task;
        }
        else return null;
    }

    public Task GetTask(int index)
    {
        if ((index >= 0) && (index < _tasks.Count))
        {
            int i = _randomIndex[index];
            Task task = _tasks[i];
            return task;
        }
        else
            return null;          
    }

    public Task NextTask()
    {
        Task task;
        int i;
        if (_taskIndex < (maxTaskNum -1)) //maxTaskNum should be set near
            //the start of the paradigm
        {
            if (_taskIndex == (_tasks.Count - 1)) //new task not in list
            {

                AddTask(); //add empty task and increases _taskIndex automatically
                task = GetTask();
            }
            else
            {
                //randomize list before starting second task (i.e. task 1)
                if (_taskIndex == 0) 
                {
                    if (bRandomize)
                        RandomizeTasks();
                    else
                        SortTasks();
                }
                _taskIndex++;
                task = GetTask();
            }
        }
        else
        {
            _taskIndex = 0;
            bEditing = false;
            //task 0 will allways be the first task. Randomisation starts with task 
            /************************
            if (bRandomize)
                RandomizeTasks();
            else
                SortTasks();
            i = _randomIndex[_taskIndex];
            task = _tasks[i];
            **************/
            task = _tasks[_taskIndex];
            task.ResetSceneIndex();
        }          
        return task;
    }

    void UpdateTask(Task task)
    {
        int i = _randomIndex[_taskIndex];
        _tasks[i] = task;
    }

    void UpdateTask(Task task, int index)
    {
        if ((index >= 0) && (index < _tasks.Count))
            _tasks[index] = task;
    }
    public int GetSceneIndex()

    {
        Task task = GetTask();
        if (task != null)
            return task.sceneIndex;
        else
            return -1;
    }

    public Scene NextScene()
    {
        Task task = GetTask();
        Scene scene = task.NextScene();
        if (scene == null)
        {
            if (bEditing)
            {
                // here we are still in the process to add scenes 
                //(for a new task we would use the new task button)
                scene = new Scene();
                task.AddScene(scene);
            }
            else
            {
                //we are replaying through all the tasks we created
                task = NextTask();
                //we have to reset the tasks scene index from number of scenes - 1 to 0 
                task.ResetSceneIndex();
                // we get the first scene
                scene = task.GetScene();
            }
        }
        return scene;
    }
    public void StoreTasks(string menuFileName)
    {
        string newContent = "";
        int itk = 0;
        int isc;
        foreach (Task task in _tasks)
        {
            isc = 0;
            foreach(Scene scene in task.scenes) { 
                foreach (SceneEntry entry in scene.sceneEntries)
                {
                    newContent += (itk.ToString() + "," + isc.ToString() + "," + entry.varName + "," + entry.val + "," + entry.time.ToString() + Environment.NewLine);
                }
                isc++;
            }
            itk++;
        }

        File.AppendAllText(menuFileName, newContent);
        //StoreTaskInJSON(menuFileName);

     }

    // menuFilename shoudl contain the full path name
    // will not be used for paradigm with tasks containing one to many scnes (i.e. sceneEntries).
    public void LoadTasks(string menuFilename)
    {
        if (File.Exists(menuFilename))
        {
            StreamReader fs = File.OpenText(menuFilename);
            string content = fs.ReadLine();
            char[] separators = { ',', ' ' };
            int taskNum = 0;
            int sceneNum = 0;
            //clear sceneEntries list
            _tasks.Clear();
            _randomIndex.Clear();
            _taskIndex = -1;
            Task task = null;
            int oldTaskNum = -1;
            int oldSceneNum = -1;
            Scene scene = null;
            
            for (;;)
            {
                if (fs.EndOfStream)
                    break;
                content = fs.ReadLine();
                string[] parts = content.Split(separators, StringSplitOptions.None);
                bool success = int.TryParse(parts[0], out taskNum);
                if (success)
                {
                    success = int.TryParse(parts[1], out sceneNum);
                    if (success)
                    {
                        if (taskNum != oldTaskNum)
                        {
                            AddTask();
                            task = _tasks[taskNum];
                            oldTaskNum = taskNum;
                            scene = task.GetScene();
                            oldSceneNum = 0;
                        }
                        else if (sceneNum != oldSceneNum)
                        {

                            scene = new Scene();
                            task.AddScene(scene);
                            oldSceneNum = sceneNum;
                        }
                        SceneEntry item = new SceneEntry(parts[2], parts[3], 0f);
                        scene.Add(item);
                    }
                }
            }
        }
        //after finishing reading all the tasks with their scenes, we have to set some variables to their starting point

        _taskIndex = 0;
        bEditing = false;
        if (bRandomize)
            RandomizeTasks();
        else
            SortTasks();
        //int i = _randomIndex[_taskIndex];
        //Task tsk = _tasks[i];

    }

    private Scene BuildDiffMenu()
    {
        //check for first scene. 
        if ((_taskIndex == 0) && (_tasks[0].sceneIndex == 0))
        {
            refMenu = GetTask(0).GetScene(0);
            add2RefMenu = new Scene();
        }
        else
        {
            Scene curScene = GetTask().GetScene();
            foreach (SceneEntry entry in curScene.sceneEntries)
                add2RefMenu.Add(entry);
        }
        return add2RefMenu;
    }

} //end class TaskList


