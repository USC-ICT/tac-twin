using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

public class AnimationClipBrowser : EditorWindow
{
    /// <summary>
    /// Class to organize the AnimationCategory by the character name
    /// </summary>
    public class AnimationCategoryList
    {
        public string m_characterName;
        public Dictionary<string, AnimationCategory> m_animationCategory;
        public bool m_show;

        public AnimationCategoryList()
        {
            m_characterName = "";
            m_animationCategory = new Dictionary<string, AnimationCategory>();
            m_show = false;
        }

        public void setCharacterName(string name)
        {
            m_characterName = name;
        }

        public string getCharacterName()
        {
            return m_characterName;
        }

        public void addAnimationCategory(string categoryName, AnimationCategory ac)
        {
            m_animationCategory.Add(categoryName, ac);
        }

        public Dictionary<string, AnimationCategory> getAnimationCategories()
        {
            return m_animationCategory;
        }
    }

    /// <summary>
    /// Class to organize the animation clip by animation clip name
    /// </summary>
    public class AnimationCategory
    {
        public string m_categoryName;
        public List<AnimationClip> m_animationClips;
        public bool m_show;

        public AnimationCategory()
        {
            m_categoryName = "";
            m_animationClips = new List<AnimationClip>();
            m_show = false;
        }

        public void setCategoryName(string name)
        {
            m_categoryName = name;
        }

        public string getCategoryName()
        {
            return m_categoryName;
        }

        public void addAnimationClip(AnimationClip ac)
        {
            m_animationClips.Add(ac);
        }

        public List<AnimationClip> getAnimationClips()
        {
            return m_animationClips;
        }
    }

    /// <summary>
    /// Member Variables
    /// </summary>
    private static AnimationClipBrowser m_editorWindow = null;
    private string m_showButtonString = "Refresh Animation List";
    private string m_animationFolderPath = "";
    private List<AnimationCategoryList> m_allLists = new List<AnimationCategoryList>();
    private Vector2 m_pos = Vector2.zero;
    private bool m_animationClipsShowing = false;
    private AnimationClip m_dragAndDropAnimationClip = null;
    private string m_dragAndDropAnimationClipPath = "";
    
    
    /// <summary>
    /// Unity GUI Functions
    /// </summary>
    [MenuItem("Tools/Animation Clip Browser")]
    static void ShowAnimationClipBrowserWindow()
    {
        m_editorWindow = (AnimationClipBrowser)EditorWindow.GetWindow(typeof(AnimationClipBrowser));
        m_editorWindow.autoRepaintOnSceneChange = true;
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        //-----------------------------------------------------------------------------------------------
        m_pos = GUILayout.BeginScrollView(m_pos, false, true, GUILayout.Width(m_editorWindow.position.width));

        m_animationFolderPath = EditorGUILayout.TextField("Path", m_animationFolderPath);

        EditorGUILayout.ObjectField("Current Animation", m_dragAndDropAnimationClip, typeof(AnimationClip), true);
        //--- Get the button press event
        if (GUILayout.Button(m_showButtonString))
        {
            if(m_animationClipsShowing == false)
            {
                ExtractFilesInfo();
                m_animationClipsShowing = true;
            }
            else
            {
                ClearFilesInfo();
                m_animationClipsShowing = false;
                m_dragAndDropAnimationClip = null;
                m_dragAndDropAnimationClipPath = "";
                m_animationFolderPath = "";
            }
        }

        //--- Display Animation Clips
        ShowAnimationClipsList();

        //--- Handle "DragAndDrop" event
        DragAndDropEventHandle();
        //-----------------------------------------------------------------------------------------------

        GUILayout.EndScrollView();
        GUILayout.EndHorizontal();
    }


    /// <summary>
    /// Helper Functions
    /// </summary>

    //--- Handle the mouse drag-and-drop event
    private void DragAndDropEventHandle()
    { 
        if (Event.current.type == EventType.MouseDrag)
        {
            //--- Create the ObjectField Rect and the mouse position to decide whether the mouse is in the ObjectField Rect
            Rect windowTopRect = new Rect(m_editorWindow.position.xMin, m_editorWindow.position.yMin + 30, m_editorWindow.position.width, 50);
            Vector2 mousePositionWorldCoordinate = new Vector2(Event.current.mousePosition.x + windowTopRect.xMin, Event.current.mousePosition.y + windowTopRect.yMin);

            if (windowTopRect.Contains(mousePositionWorldCoordinate))
            {
                if (m_dragAndDropAnimationClipPath != "")
                {
                    //--- Clear out drag data
                    DragAndDrop.PrepareStartDrag();

                    //--- Get the fbx and its animation clip
                    Object[] fbx = AssetDatabase.LoadAllAssetsAtPath(m_dragAndDropAnimationClipPath);
                    AnimationClip target = null;
                    foreach (Object o in fbx)
                    {
                        if (o is AnimationClip)
                        {
                            target = (AnimationClip)o;
                            break;
                        }
                    }
                    Debug.Log("*** AnimationClip: " + target.ToString() + " Exists ***");
                    DragAndDrop.objectReferences = new Object[1] { (Object)target };

                    //--- Start the actual drag
                    DragAndDrop.StartDrag(target.name);

                    //--- Make sure no one uses the event after us
                    Event.current.Use();
                }
            }
        }
        
    }

    //--- Display the animation clips in Unity GUI window
    private void ShowAnimationClipsList()
    {
        if (m_allLists.Count > 0)
        {
            int i = 0;
            foreach (AnimationCategoryList al in m_allLists)
            {
                string characterName = al.getCharacterName();

                al.m_show = EditorGUILayout.Foldout(al.m_show, characterName);

                if (al.m_show)
                {
                    Dictionary<string, AnimationCategory> ac = al.getAnimationCategories();

                    foreach (KeyValuePair<string, AnimationCategory> pair in ac)
                    {
                        string categoryName = pair.Key;
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(30.0f);
                        pair.Value.m_show = EditorGUILayout.Foldout(pair.Value.m_show, categoryName);
                        GUILayout.EndHorizontal();
                        if (pair.Value.m_show)
                        {
                            AnimationCategory category = pair.Value;
                            List<AnimationClip> clips = category.getAnimationClips();
                            foreach (AnimationClip clip in clips)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(60.0f);
                                //EditorGUILayout.ObjectField("", clip, typeof(AnimationClip), true);
                                string animationClipButtonName = clip.ToString().Split('(')[0];
                                if (GUILayout.Button(animationClipButtonName))
                                {
                                    m_dragAndDropAnimationClip = clip;
                                    m_dragAndDropAnimationClipPath = AssetDatabase.GetAssetPath(clip);
                                    Debug.Log("Choosed Animation Clip Path: " + m_dragAndDropAnimationClipPath);
                                }
                                GUILayout.EndHorizontal();
                            }
                        }
                    }

                }
                i++;
            }
        }
    }

    //--- Exact and store the animation clips in m_allLists organically
    private void ExtractFilesInfo()
    {
        if (m_animationFolderPath != null && m_animationFolderPath.Length > 0)
        {
            //---Fix if the user forgets "/"           
            string lastChar = m_animationFolderPath.Substring(m_animationFolderPath.Length - 1);
            if (!lastChar.Equals("/"))
            {
                m_animationFolderPath += "/";
            }
            
            DirectoryInfo dir = new DirectoryInfo(m_animationFolderPath);
            if (dir.Exists)
            {
                DirectoryInfo[] folders = dir.GetDirectories("Chr*");
                
                foreach (DirectoryInfo di in folders)
                {
                    AnimationCategoryList acl = new AnimationCategoryList();
                    acl.setCharacterName(di.Name.ToString());

                    FileInfo[] files = di.GetFiles("*.fbx");
                    
                    foreach (FileInfo fi in files)
                    {
                        //---Extract the animation clip
                        string fbxLoc = m_animationFolderPath + di.Name.ToString() + "/" + fi.Name.ToString();
                        Object[] fbx = AssetDatabase.LoadAllAssetsAtPath(fbxLoc);
                        AnimationClip target = null;
                        foreach (Object o in fbx)
                        {
                            if (o is AnimationClip)
                            {
                                target = (AnimationClip)o;
                                break;
                            }
                        }

                        AnimationCategory animationCategory = new AnimationCategory();

                        //---Split file name 
                        string animationNameNoExtension = fi.Name.ToString().Replace(".fbx", "");
                        string animationName = animationNameNoExtension.ToString().Split('@')[1]; //Remove the character name
                        string animationNameFront = animationName.Split('_')[0]; //Get the string before '_'

                        int intName;
                        if (int.TryParse(animationNameFront, out intName))
                        {
                            if (!acl.m_animationCategory.ContainsKey("Number"))
                            {
                                animationCategory.setCategoryName("Number");
                                if (target != null) animationCategory.addAnimationClip(target);
                                acl.addAnimationCategory("Number", animationCategory);
                            }
                            else
                            {
                                if (target != null) acl.m_animationCategory["Number"].addAnimationClip(target);
                            }
                        }
                        else
                        {
                            string animationNameFrontNoNum = Regex.Replace(animationNameFront, @"\d", "");

                            if (!acl.m_animationCategory.ContainsKey(animationNameFrontNoNum))
                            {
                                animationCategory.setCategoryName(animationNameFrontNoNum);
                                if (target != null) animationCategory.addAnimationClip(target);
                                acl.addAnimationCategory(animationNameFrontNoNum, animationCategory);
                            }
                            else
                            {
                                if (target != null) acl.m_animationCategory[animationNameFrontNoNum].addAnimationClip(target);
                            }
                        }
                    } // Categorize all files in one folder

                    m_allLists.Add(acl);
                }
            }
            else
            {
                Debug.Log("Directory Doesn't Exist");
            }
        }
    }

    //--- Clear all content in m_allLists
    private void ClearFilesInfo()
    {
        m_allLists.Clear();
    }   
}

