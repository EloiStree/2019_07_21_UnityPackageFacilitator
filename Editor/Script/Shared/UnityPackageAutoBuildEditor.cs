﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
//https://docs.unity3d.com/Packages/com.unity.package-manager-ui@1.8/manual/index.html

[CustomEditor(typeof(UnityPackageAutoBuild))]
public class UnityPackageAutoBuildEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UnityPackageAutoBuild myScript = (UnityPackageAutoBuild)target;

        string projectName = GetProjectName(myScript);
        string whereToCreate = GetWhereToCreateProject(myScript);
        bool isGitDirectoryDefined = Directory.Exists(whereToCreate + "/.git/");
        bool isGitUrlDefined = GetGitUrlDefined(myScript);

        GUILayout.BeginHorizontal();
        if (isGitUrlDefined && !isGitDirectoryDefined && GUILayout.Button("Clone"))
        {
            Directory.CreateDirectory(whereToCreate);
            QuickGit.Clone(myScript.m_gitLink, whereToCreate);
            CreateStructure(myScript);
        }
        GUILayout.EndHorizontal();
        if (!isGitDirectoryDefined && !isGitUrlDefined)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Create: Git Project");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("GitLab"))
            {
                Application.OpenURL("https://gitlab.com/projects/new");
            }
            if (GUILayout.Button("Github"))
            {
                Application.OpenURL("https://github.com/new");
            }
            if (GUILayout.Button("Local"))
            {
                UnityEngine.Debug.Log(">>"+myScript.GetFolderPath());
                QuickGit.CreateLocal(myScript.GetFolderPath());
                myScript.
        MakeSureThatPullPushScriptIsAssociatedToThisScript();
            }
            GUILayout.EndHorizontal();
        }
        if (isGitDirectoryDefined && !isGitUrlDefined)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Create: Create project");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            myScript.m_gitUserName = EditorGUILayout.TextField("User:", myScript.m_gitUserName);
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();

            if (GUILayout.Button("GitLab"))
            {
                QuickGit.PushLocalToGitLab(whereToCreate, myScript.m_gitUserName, GetProjectDatedNameId(myScript), out myScript.m_gitLink);
                myScript.
      MakeSureThatPullPushScriptIsAssociatedToThisScript();
            }
            //if (GUILayout.Button("Github"))
            //{
            //    QuickGit.PushLocalToGitHub(whereToCreate, myScript.m_gitUserName, projectName, out myScript.m_gitLink);
            //}

            GUILayout.EndHorizontal();
        }
        if (isGitDirectoryDefined)
        {
            GUILayout.BeginHorizontal();
            bool packageExist = File.Exists(whereToCreate + "/package.json");
            if (GUILayout.Button(packageExist ? "Create structure" : "Update structure"))
            {
                CreateStructure(myScript);
                QuickGit.AddFileInEmptyFolder(whereToCreate);
            }

            GUILayout.EndHorizontal();
        }

        EditorGUILayout.HelpBox("Reminder: Git must be install and Git.exe must be add in System Variable Path.", MessageType.Warning, true);

    }

    private static string GetProjectDatedNameId(UnityPackageAutoBuild myScript)
    {
        return myScript.m_packageJson.m_projectId.GetProjectDatedNameId(false);
    }

    private static bool GetGitUrlDefined(UnityPackageAutoBuild myScript)
    {
        return !string.IsNullOrEmpty(myScript.m_gitLink);
    }

    private static string GetWhereToCreateProject(UnityPackageAutoBuild myScript)
    {
        return myScript.m_projectPath + "/" + myScript.m_packageJson.GetProjectDatedId(false);
    }

    private static string GetProjectName(UnityPackageAutoBuild myScript)
    {
        return myScript.m_packageJson.m_projectId.GetProjectNameWithoutSpace();
    }

    private void CreateLocalGit(UnityPackageAutoBuild myScript,string where)
    {
        QuickGit.CreateLocal(where + "/" + GetProjectDatedNameId(myScript));
    }

    public void CreateStructure(UnityPackageAutoBuild myScript)
    {
        myScript.MakeSureThatTheAssemblyEditorTargetTheRuntimeOne();

        string whereToCreate = myScript.m_projectPath + "/" + GetProjectDatedNameId(myScript);
        QuickGit.AddFileInEmptyFolder(whereToCreate);
        CreatePackageJson(myScript.m_packageJson, whereToCreate, myScript);
        CreateFolders(whereToCreate, myScript.m_directoriesStructure);
        CreateAssembly(myScript.m_packageJson.m_assemblyRuntime, whereToCreate);
        CreateAssembly(myScript.m_packageJson.m_assemblyEditor, whereToCreate);
        File.WriteAllText(whereToCreate + "/requiredpackages.json", myScript.m_packageJson.m_classicUnityPackageRequired.ToJson());
        AssetDatabase.Refresh();
    }

    public void CreateAssembly( UnityPackageAssemblyBuilderJson assemblyInfo, string whereToCreate)
    {
        string[] dependenciesModificatedForJson = new string[assemblyInfo.m_reference.Length];
        for (int i = 0; i < assemblyInfo.m_reference.Length; i++)
        {
            dependenciesModificatedForJson[i] = "\"" + assemblyInfo.m_reference[i] + "\"";
        }

        string packageJson = "";
        packageJson += "\n{                                                                   ";
        packageJson += "\n            \"name\": \"" + assemblyInfo.m_packageNamespaceId + "\",          ";
        packageJson += "\n    \"references\": [";
        //       packageJson += "\n        \"be.eloiexperiments.randomtool\"                             ";

        packageJson += string.Join(",", dependenciesModificatedForJson);
        packageJson += "\n],                                                              ";
        packageJson += "\n    \"optionalUnityReferences\": [],                                  ";
        if (assemblyInfo.m_isEditorAssembly)
        {
            packageJson += "\n    \"includePlatforms\": [                                           ";
            packageJson += "\n        \"Editor\"                                                    ";
            packageJson += "\n    ],                                                              ";
        }
        else
        {
            packageJson += "\n    \"includePlatforms\": [],                                                  ";
        }
        packageJson += "\n    \"excludePlatforms\": [],                                         ";
        packageJson += "\n    \"allowUnsafeCode\": false,                                       ";
        packageJson += "\n    \"overrideReferences\": false,                                    ";
        packageJson += "\n    \"precompiledReferences\": [],                                    ";
        packageJson += "\n    \"autoReferenced\": true,                                         ";
        packageJson += "\n    \"defineConstraints\": []                                         ";
        packageJson += "\n}                                                                   ";

        if (assemblyInfo.m_isEditorAssembly)
        {
            Directory.CreateDirectory(whereToCreate + "/Editor");
            string name = whereToCreate + "/Editor/com.unity." + assemblyInfo.m_packageName + ".Editor.asmdef";
            File.Delete(name);
            File.WriteAllText(name, packageJson);

        }
        else
        {
            Directory.CreateDirectory(whereToCreate + "/Runtime/");
            string name = whereToCreate + "/Runtime/com.unity." + assemblyInfo.m_packageName + ".Runtime.asmdef";
            File.Delete(name);
            File.WriteAllText(name, packageJson);

        }



    }





    private void CreateFolders(string whereToCreate, string[] structure)
    {
        for (int i = 0; i < structure.Length; i++)
        {
            Directory.CreateDirectory(whereToCreate + "/" + structure[i]);

        }
    }

    private void CreatePackageJson( UnityPackageBuilderJson packageInfo, string whereToCreate, UnityPackageAutoBuild additionalInfo, string fileName = "package.json")
    {
        string packageJson = "";
        string[] dependenciesModificatedForJson = new string[packageInfo.m_dependencies.Length];
        for (int i = 0; i < packageInfo.m_dependencies.Length; i++)
        {
            dependenciesModificatedForJson[i] = "\"" + packageInfo.m_dependencies[i] + "\": \"0.0.1\"";
        }
        string[] keywordForJson = new string[packageInfo.m_keywords.Length];
        for (int i = 0; i < packageInfo.m_keywords.Length; i++)
        {
            keywordForJson[i] = "\"" + packageInfo.m_keywords[i] + "\"";
        }

        packageJson += "\n{                                                                                ";
        packageJson += "\n  \"name\": \"" + packageInfo.GetProjectNamespaceId(true) + "\",                              ";
        packageJson += "\n  \"displayName\": \"" + packageInfo.m_projectId.GetProjectDisplayName() + "\",                        ";
        packageJson += "\n  \"version\": \"" + packageInfo.m_packageVersion + "\",                         ";
        packageJson += "\n  \"unity\": \"" + packageInfo.m_unityVersion + "\",                             ";
        packageJson += "\n  \"description\": \"" + packageInfo.m_description + "\",                         ";
        packageJson += "\n  \"keywords\": [" + string.Join(",", keywordForJson) + "],                       ";
        packageJson += "\n  \"category\": \"" + packageInfo.m_category.ToString() + "\",                   ";
        packageJson += "\n  \"dependencies\":{" + string.Join(",", dependenciesModificatedForJson) + "}     ";
        packageJson += "\n  }                                                                                ";
        Directory.CreateDirectory(whereToCreate);
        File.Delete(whereToCreate + "/package.json");
        File.WriteAllText(whereToCreate + "/package.json", packageJson);

        string m_howToUse = "# How to use: " + packageInfo.GetProjectDatedId(false) + "   " ;
        m_howToUse += "\n   ";

        m_howToUse += "\nAdd the following line to the [UnityRoot]/Packages/manifest.json    ";
        m_howToUse += "\n``` json     ";
        m_howToUse += "\n" + string.Format("\"{0}\":\"{1}\",", packageInfo.GetProjectNamespaceId(true), additionalInfo.m_gitLink) +  "    "  ;
        m_howToUse += "\n```    ";
        m_howToUse += "\n--------------------------------------    ";
        m_howToUse += "\n   ";
        m_howToUse += "\nFeel free to support my work: " + additionalInfo.m_patreonLink+"   ";
        m_howToUse += "\nContact me if you need assistance: " + additionalInfo.m_contact + "   ";
        m_howToUse += "\n   ";
        m_howToUse += "\n--------------------------------------    ";
        m_howToUse += "\n``` json     ";
        m_howToUse += packageJson;
        m_howToUse += "\n```    ";

        File.WriteAllText(whereToCreate + "/readme.md", m_howToUse);

    }
}



public static class UnityPackageManagerUtility
{
    [MenuItem("Window /Package Utility / Remove Locker")]
    public static void RemoveLocker()
    {
        //string packagePath = GetProjectPackagePath();
        //string package = File.ReadAllText(packagePath);
        //package = Regex.Replace(package, "(,)[. \\n \\r]*(\"lock\":)[\\S \\r \\n { }]*", "}");
        // File.WriteAllText(packagePath, package);
        //AssetDatabase.Refresh();
        UnityPackageUtility.RemoveLocker();
    }

    private static string GetProjectPackagePath()
    {
        return Application.dataPath + "/../Packages/manifest.json";
    }
}


//// com.unity.unityprefsthemall.Runtime
