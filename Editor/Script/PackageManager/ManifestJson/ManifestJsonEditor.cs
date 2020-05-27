﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ManifestJsonEditor : EditorWindow
{
    public Info m_info = new Info();
    public class Info
    {
        internal Utility_ManifestJson m_manifestInfo = new Utility_ManifestJson();
        internal string m_manifestaddNamespace="";
        internal string m_manifestaddgitlink="";
    }
    [MenuItem("Window/Package Utility/Manifest")]
    static void Init()
    {
        ManifestJsonEditor window = (ManifestJsonEditor)EditorWindow.GetWindow(typeof(ManifestJsonEditor));
        window.titleContent = new GUIContent("Manifest Configuration") ;
        window.Show();
    }
    void OnGUI()
    {
        GUILayout.Label("Manifest Configuration", EditorStyles.boldLabel);
     

        UnityPackageEditorDrawer.DrawManifrest(ref m_info.m_manifestInfo, ref m_info.m_manifestaddNamespace, ref m_info.m_manifestaddgitlink);
    }
}