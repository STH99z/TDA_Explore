using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;

// Editor window for listing all object reference curves in an animation clip
public class 动画信息 : EditorWindow
{
    private AnimationClip clip;

    [MenuItem("工具/获取动画Clip信息")]
    static void Init()
    {
        GetWindow(typeof(动画信息));
    }

    public void OnGUI()
    {
        clip = EditorGUILayout.ObjectField("Clip", clip, typeof(AnimationClip), false) as AnimationClip;

        EditorGUILayout.LabelField("Object reference curves:");
        if (clip != null)
        {
            int i = 1;
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                EditorGUILayout.LabelField(i.ToString());
                i++;
                ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                string s = "";
                s += "binding.path=" + binding.path + "\n";
                s += "binding.propertyName=" + binding.propertyName + "\n";
                s += "binding.type=" + binding.type.ToString() + "\n";
                s += "keyframes.Length=" + keyframes.Length;
                EditorGUILayout.TextArea(s);
                EditorGUILayout.BeginHorizontal();
                for (int j=0;j<keyframes.Length;j++)
                {
                    EditorGUILayout.ObjectField(keyframes[j].value, keyframes[j].value.GetType(), false);
                }
                EditorGUILayout.EndHorizontal();

            }
        }
        EditorGUILayout.LabelField("Curves:");
        if (clip != null)
        {
            int i = 1;
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                EditorGUILayout.LabelField(i.ToString());
                i++;    
                string s = "";
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                s += "binding.path=" + binding.path + "\n";
                s += "binding.propertyName=" + binding.propertyName + "\n";
                s += "binding.type=" + binding.type.ToString() + "\n";
                s += "curve.keys.Length=" + curve.keys.Length;
                EditorGUILayout.TextArea(s);
                s = "";
                int ii = 0;
                foreach (var k in curve.keys)
                {
                    s += ii + " time=" + k.time + " value=" + k.value + "\n";
                    ii++;
                }
                EditorGUILayout.TextArea(s);
            }
        }
    }
}

public class 角色动画生成器 : EditorWindow
{ 
    Sprite spritesheet;
    Sprite[] sprites;

    int iFramesPerLoop = 3;
    int iTotalLoops = 5;
    int iIdleFrameForLoop = 2;
    int iSamples = 5;
    bool bMirrorLR234 = true ;
    bool bIdleForLoop = true ;
    string[] clipNames =
    {
        "1_U",
        "2_UR",
        "8_UL",
        "3_R",
        "7_L",
        "4_DR",
        "6_DL",
        "5_D"
    };
	string[] clipNamesIdle =
	{
		"9_U",
		"10_UR",
		"16_UL",
		"11_R",
		"15_L",
		"12_DR",
		"14_DL",
		"13_D"
	};
	string characterName;
    string directory;

    [MenuItem("工具/测试工具")]
    static void test()
    {
        Debug.ClearDeveloperConsole();
        Debug.Log(">Test");
    }

    [MenuItem("工具/角色动画生成")]
    static void OpenWindow()
    {
        GetWindow<角色动画生成器>();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("一个角色的切好的Sprite（任选一个）");
        spritesheet = EditorGUILayout.ObjectField(spritesheet, typeof(Sprite), false) as Sprite;
        EditorGUILayout.LabelField("每个Walk动画的帧数");
        iFramesPerLoop = EditorGUILayout.IntField(iFramesPerLoop);
        EditorGUILayout.LabelField("Sprite中总共动画个数");
        iTotalLoops = EditorGUILayout.IntField(iTotalLoops);
        bMirrorLR234 = EditorGUILayout.ToggleLeft("第2,3,4个动画生成左右对称的两个动画", bMirrorLR234);
        bIdleForLoop = EditorGUILayout.ToggleLeft("给每个动画创建一个Idle动画", bIdleForLoop);
        EditorGUILayout.LabelField("Idle动画单帧位于每个动画中的位置（从1开始）");
        iIdleFrameForLoop = EditorGUILayout.IntField(iIdleFrameForLoop);
        EditorGUILayout.LabelField("每秒帧数（Samples）");
        iSamples = EditorGUILayout.IntField(iSamples);

        if (GUILayout.Button("载入生成"))
        {
            string sheetPath = AssetDatabase.GetAssetPath(spritesheet);
            //Assets/Resources/Sprites/Character/chara_01.png
            characterName = Path.GetFileNameWithoutExtension(sheetPath);
            //chara_01
            directory = Path.GetDirectoryName(sheetPath);
            //Assets/Resources/Sprites/Character/

            sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToArray();

            if (!bMirrorLR234)
            {
                //原本的代码
                for (int i = 0; i < iTotalLoops; i++)
                {
                    AnimationClip clip = new AnimationClip();
                    clip.frameRate = iFramesPerLoop;

                    EditorCurveBinding curveBinding = new EditorCurveBinding();
                    curveBinding.path = "";
                    curveBinding.propertyName = "m_Sprite";
                    curveBinding.type = typeof(SpriteRenderer);

                    ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[iFramesPerLoop];
                    for (int j = 0; j < iFramesPerLoop; j++)
                    {
                        keys[j] = new ObjectReferenceKeyframe();
                        keys[j].time = (1.0f / iFramesPerLoop) * j;
                        keys[j].value = sprites[iFramesPerLoop * i + j];
                    }

                    AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keys);
                    AssetDatabase.CreateAsset(clip, directory + "/" + characterName + "_" + clipNames[i] + ".anim");
                }
            }
            else
            {
                //234要生成镜像的代码
                for (int i = 0; i < iTotalLoops + 3; i++)
                {
                    int targetKey;
                    targetKey = (i + 1) / 2;
                    //0 1 2 3 4 5 6 7
                    //covert to
                    //0 1 1 2 2 3 3 4

                    AnimationClip clip = new AnimationClip();
                    clip.frameRate = iSamples;

                    EditorCurveBinding curveBinding = new EditorCurveBinding();
                    curveBinding.path = "";
                    curveBinding.propertyName = "m_Sprite";
                    curveBinding.type = typeof(SpriteRenderer);

                    ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[iFramesPerLoop + 1];
                    for (int j = 0; j < iFramesPerLoop; j++)
                    {
                        keys[j] = new ObjectReferenceKeyframe();
                        keys[j].time = (1.0f / iSamples) * j;
                        keys[j].value = sprites[iFramesPerLoop * targetKey + j];
                    }
                    keys[3] = new ObjectReferenceKeyframe();
                    keys[3].time = (1.0f / iSamples) * 3;
                    keys[3].value = sprites[iFramesPerLoop * targetKey + 1];

                    switch (i)
                    {
                        case 2:
                        case 4:
                        case 6:
                            EditorCurveBinding cb2 = new EditorCurveBinding();
                            cb2.path = "";
                            cb2.propertyName = "m_FlipX";
                            cb2.type = typeof(SpriteRenderer);
                            AnimationCurve ac = new AnimationCurve();
                            ac.AddKey(0f, 1f);
                            AnimationUtility.SetEditorCurve(clip, cb2, ac);
                            Debug.Log(i);
                            break;
                    }


                    AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keys);
                    AssetDatabase.CreateAsset(clip, directory + "/" + characterName + "_" + clipNames[i] + ".anim");
                }
                if (bIdleForLoop)
                {
                    for (int i = 0; i < iTotalLoops + 3; i++)
                    {
                        int targetKey;
                        targetKey = (i + 1) / 2;
                        //0 1 2 3 4 5 6 7
                        //covert to
                        //0 1 1 2 2 3 3 4

                        AnimationClip clip = new AnimationClip();
                        clip.frameRate = iSamples;

                        EditorCurveBinding curveBinding = new EditorCurveBinding();
                        curveBinding.path = "";
                        curveBinding.propertyName = "m_Sprite";
                        curveBinding.type = typeof(SpriteRenderer);

                        ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[1];
                        keys[0] = new ObjectReferenceKeyframe();
                        keys[0].time = 0f;
                        keys[0].value = sprites[iFramesPerLoop * targetKey + 1];
                        switch (i)
                        {
                            case 2:
                            case 4:
                            case 6:
                                EditorCurveBinding cb2 = new EditorCurveBinding();
                                cb2.path = "";
                                cb2.propertyName = "m_FlipX";
                                cb2.type = typeof(SpriteRenderer);
                                AnimationCurve ac = new AnimationCurve();
                                ac.AddKey(0f, 1f);
                                AnimationUtility.SetEditorCurve(clip, cb2, ac);
                                Debug.Log(i);
                                break;
                        }

                        AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keys);
                        AssetDatabase.CreateAsset(clip, directory + "/" + characterName + "_" + clipNamesIdle[i] + "_Idle.anim");
                    }
                }
            }
            string[] folders = new string[1];
            folders[0] = directory;
            string[] clipPaths = AssetDatabase.FindAssets(characterName, folders);

            List<AnimationClip> clips = new List<AnimationClip>();
            for (int i = 0; i < clipPaths.Length; i++)
            {
                AnimationClip c = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(clipPaths[i]), typeof(AnimationClip)) as AnimationClip;
                if (c != null)
                    clips.Add(c);
            }

            for (int i = 0; i < clips.Count; i++)
            {
                SerializedObject serializedClip = new SerializedObject(clips[i]);
                AnimationClipSettings clipSettings = new AnimationClipSettings(serializedClip.FindProperty("m_AnimationClipSettings"));

                clipSettings.loopTime = true;

                serializedClip.ApplyModifiedProperties();
            }

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(directory + "/" + characterName + "_AnimatorController.controller");
            controller.AddParameter("X", AnimatorControllerParameterType.Float);
			controller.AddParameter("Y", AnimatorControllerParameterType.Float);

			BlendTree tree;
            controller.CreateBlendTreeInController("Walk", out tree, 0);
			tree.blendType = BlendTreeType.SimpleDirectional2D;
            tree.blendParameter = "X";
			tree.blendParameterY = "Y";
            tree.useAutomaticThresholds = false;
			int cc = (bIdleForLoop ? clips.Count / 2 : clips.Count);
			for (int i = 0; i < cc; i++) 
            {
				double d = i * Mathf.PI * 2 / cc;
				float x, y;
				y = (float)System.Math.Cos(d);
				x = (float)System.Math.Sin(d);
				tree.AddChild(clips[i], new Vector2(x, y));
            }

            GameObject go = GameObject.Find(characterName);
            if (go == null)
            {
                go = new GameObject(characterName);
                go.AddComponent<SpriteRenderer>().sprite = sprites[0];
                go.AddComponent<Animator>().runtimeAnimatorController = controller;
            }
            else
            {
                SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                if (sr == null)
                    go.AddComponent<SpriteRenderer>().sprite = sprites[0];
                else
                    sr.sprite = sprites[0];

                Animator anim = go.GetComponent<Animator>();
                if (anim == null)
                    go.AddComponent<Animator>().runtimeAnimatorController = controller;
                else
                    anim.runtimeAnimatorController = controller;
            }

            PrefabUtility.CreatePrefab(directory + "/" + characterName + ".prefab", go);
        }

    }
}

class AnimationClipSettings
{
    SerializedProperty m_Property;

    private SerializedProperty Get(string property) { return m_Property.FindPropertyRelative(property); }

    public AnimationClipSettings(SerializedProperty prop) { m_Property = prop; }

    public float startTime { get { return Get("m_StartTime").floatValue; } set { Get("m_StartTime").floatValue = value; } }
    public float stopTime { get { return Get("m_StopTime").floatValue; } set { Get("m_StopTime").floatValue = value; } }
    public float orientationOffsetY { get { return Get("m_OrientationOffsetY").floatValue; } set { Get("m_OrientationOffsetY").floatValue = value; } }
    public float level { get { return Get("m_Level").floatValue; } set { Get("m_Level").floatValue = value; } }
    public float cycleOffset { get { return Get("m_CycleOffset").floatValue; } set { Get("m_CycleOffset").floatValue = value; } }

    public bool loopTime { get { return Get("m_LoopTime").boolValue; } set { Get("m_LoopTime").boolValue = value; } }
    public bool loopBlend { get { return Get("m_LoopBlend").boolValue; } set { Get("m_LoopBlend").boolValue = value; } }
    public bool loopBlendOrientation { get { return Get("m_LoopBlendOrientation").boolValue; } set { Get("m_LoopBlendOrientation").boolValue = value; } }
    public bool loopBlendPositionY { get { return Get("m_LoopBlendPositionY").boolValue; } set { Get("m_LoopBlendPositionY").boolValue = value; } }
    public bool loopBlendPositionXZ { get { return Get("m_LoopBlendPositionXZ").boolValue; } set { Get("m_LoopBlendPositionXZ").boolValue = value; } }
    public bool keepOriginalOrientation { get { return Get("m_KeepOriginalOrientation").boolValue; } set { Get("m_KeepOriginalOrientation").boolValue = value; } }
    public bool keepOriginalPositionY { get { return Get("m_KeepOriginalPositionY").boolValue; } set { Get("m_KeepOriginalPositionY").boolValue = value; } }
    public bool keepOriginalPositionXZ { get { return Get("m_KeepOriginalPositionXZ").boolValue; } set { Get("m_KeepOriginalPositionXZ").boolValue = value; } }
    public bool heightFromFeet { get { return Get("m_HeightFromFeet").boolValue; } set { Get("m_HeightFromFeet").boolValue = value; } }
    public bool mirror { get { return Get("m_Mirror").boolValue; } set { Get("m_Mirror").boolValue = value; } }
}