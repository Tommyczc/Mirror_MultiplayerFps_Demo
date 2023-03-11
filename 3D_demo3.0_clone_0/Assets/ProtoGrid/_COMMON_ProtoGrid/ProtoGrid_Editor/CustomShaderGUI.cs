using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Rendering;
using UnityEditor;

public class CustomShaderGUI : ShaderGUI
{
    #region KEYWORD RENAME
    //Recup KeyWord Shader and rename it
    [SerializeField] private const string ST_WorldGridPosition = "Boolean_4ab46abd98824840ad288956590714b7";
    [SerializeField] private const string ST_EnableDistinction = "Boolean_0eafa482eff94a28b35bf16c613d91cf";

    [SerializeField] private const string ST_Grid_Texture = "Texture2D_870e997c6b8b404a8e0183080c93b21a";

    [SerializeField] private const string ST_Display_Number = "Boolean_e480d159d08d46a5bdcfb88cd26cab43";
    [SerializeField] private const string ST_EmptyTexture = "Texture2D_d81de00e90ee499eb5891f370b5f48dc";
    [SerializeField] private const string ST_Number_Texture = "Texture2D_b3c9ed9ea4b14b2aac394b39d2d2efb8";
    [SerializeField] private const string ST_Ground_BackGround_Texture = "Texture2D_6331c4bf96a740f585a3638b9cbf8ded";
    [SerializeField] private const string ST_Wall_BackGround_Texture = "Texture2D_10fc64325a0b443caba81a04cbe647cf";

    [SerializeField] private const string ST_GridSize = "Vector1_f334b731c5ca4982b4b1620a11b8ab0a";
    [SerializeField] private const string ST_Ground_BackGround_Size = "Vector1_1";
    [SerializeField] private const string ST_Wall_BackGround_Size = "Vector1_8e2439f7354045a1b8e60483f62b3bd7";

    [SerializeField] private const string ST_Number_Offset_X = "Vector1_c6314feeee774511a1126bfc28bec311";
    [SerializeField] private const string ST_Number_Offset_Y = "Vector1_c6314feeee774511a1126bfc28bec311_1";

    [SerializeField] private const string ST_Emissive_Grid = "Boolean_eefac39ca5174d7ab663eeff29205636";
    [SerializeField] private const string ST_Emissive_Power = "Vector1_2b86831e4ea5440c9be179b3d385a820";
    [SerializeField] private const string ST_Ground_Grid_Color = "Color_dce5cc796db748e687528717bc9c37d4";
    [SerializeField] private const string ST_Ground_BackGround_Color = "Color_a1820c702c334315b08860d90b891942";
    [SerializeField] private const string ST_Wall_Grid_Color = "Color_dce5cc796db748e687528717bc9c37d4_1";
    [SerializeField] private const string ST_Wall_BackGround_Color = "Color_0d0db932ceca43669cdbbf811e5ec6df";
    [SerializeField] private const string ST_BackGround_Intensity = "Vector1_8c7ee47a811749bea3603e1db1feca16";

    [SerializeField] private const string ST_Exp_Slope_Tolerance = "Vector1_43edc993bcfc49b2a9009d2d41b3db48";
    [SerializeField] private const string ST_Exp_InvertGrid = "Boolean_7e6d29e5e65841dead75911f9e0dc049";
    #endregion

    #region PARAMETERS
    // Editor Initial parameter
    [SerializeField] private int smallSpacing = 5;
    [SerializeField] private Texture2D gridPreview;


    //Keyword Parameters ---------- Preview
    [SerializeField] private bool showingPreview;


    //Keyword Parameters ---------- General Properties
    [SerializeField] private bool showingGeneralProperties;
    [SerializeField] private int worldGridPosition_INT;
    [SerializeField] private bool worldGridPosition;

    [SerializeField] private int enableDistinction_INT;
    [SerializeField] private bool enableDistinction;
    [SerializeField] private float slopeTolerance;

    //KeyWord Parameters ---------- Grid Section
    [SerializeField] private bool showingGrid;

    [SerializeField] private Texture gridTexture;
    [SerializeField] private float gridSize;
    [SerializeField] private Color groundGridColor;

    [SerializeField] private int emissiveGrid_INT;
    [SerializeField] private bool emissiveGrid;
    [SerializeField] private float emissivePower;

    [SerializeField] private Texture groundBackGroundTexture;
    [SerializeField] private float groundBackGroundSize;
    [SerializeField] private Color groundBackGroundColor;
    [SerializeField] private float backgroundIntensity;

    [SerializeField] private int displayNumber_INT;
    [SerializeField] private bool displayNumber;

    [SerializeField] private Texture numberTexture;
    [SerializeField] private float numberXoffset;
    [SerializeField] private float numberYoffset;
    [SerializeField] private int invertGrid_INT;
    [SerializeField] private bool invertGrid;
    [SerializeField] private Texture2D emptyNumberTexture;

    //KeyWord Parameters ---------- Wall Grid 
    [SerializeField] private bool showingWallGrid;

    [SerializeField] private Color wallGridColor;
    [SerializeField] private Texture wallBackGroundTexture;
    [SerializeField] private float wallBackGroundSize;
    [SerializeField] private Color wallBackGroundColor;

    //Keyword Parameters ---------- Experimental Section
    [SerializeField] private bool showingExperimental;

    // --------- DEBUG Section 
    [SerializeField] private bool showingDebug;
    [SerializeField] private bool showingBasic;

    // --------- TESTS Section 
    [SerializeField] private bool showingTest;
    #endregion

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        #region SETUP
        //_______SETUP
        Material targetMat = materialEditor.target as Material;
        gridPreview = AssetPreview.GetAssetPreview(targetMat);

        emptyNumberTexture = Resources.Load<Texture2D>("T_Empty");
        targetMat.SetTexture(ST_EmptyTexture, emptyNumberTexture);
        #endregion

        #region TITLE
        //_______TITLE
        EditorGUILayout.LabelField("PROTOGRID PARAMETERS", EditorStyles.toolbarButton);
        //Spacing
        EditorGUILayout.Space(smallSpacing); EditorGUILayout.Space(smallSpacing);
        #endregion

        #region PREVIEW
        //________SECTION - PREVIEW
        //Header Foldout - PREVIEW
        CoreEditorUtils.DrawSplitter();
        showingPreview = CoreEditorUtils.DrawHeaderFoldout("PREVIEW", showingPreview, false, null);


        // Showing properties - General Properties
        if (showingPreview)
        {
            // Grid Preview
            GUILayout.Label(gridPreview, GUI.skin.button);
            //Spacing
            EditorGUILayout.Space(smallSpacing);
        }
        else
        {

        }
        #endregion

        #region GENERAL PROPERTIES
        //________SECTION - GENERAL PROPERTIES
        //Header Foldout - GENERAL PROPERTIES
        CoreEditorUtils.DrawSplitter();
        showingGeneralProperties = CoreEditorUtils.DrawHeaderFoldout("GENERAL PROPERTIES", showingGeneralProperties, false, null);
        // Enable Wall & Ground Distinction
        // Detect current State of Distinction
        enableDistinction_INT = targetMat.GetInt(ST_EnableDistinction);
        if (enableDistinction_INT == 1)
        {
            enableDistinction = true;
        }
        else
        {
            enableDistinction = false;
        }
        // Showing properties - GENERAL PROPERTIES
        if (showingGeneralProperties)
        {
            // Label General Grid
            EditorGUILayout.Space(smallSpacing);
            EditorGUILayout.LabelField("Grid Type", EditorStyles.helpBox);
            // Grid Position
            // Detect current State of grid position
            worldGridPosition_INT = targetMat.GetInt(ST_WorldGridPosition);
            if (worldGridPosition_INT == 1)
            {
                worldGridPosition = true;
            }
            else
            {
                worldGridPosition = false;
            }
            // Display toggle world grid
            worldGridPosition = EditorGUILayout.Toggle("World Grid", worldGridPosition, GUILayout.MinWidth(180f), GUILayout.MaxWidth(3000), GUILayout.Height(20));
            if (worldGridPosition)
            {
                worldGridPosition_INT = 1;
            }
            if (!worldGridPosition)
            {
                worldGridPosition_INT = 0;
            }
            targetMat.SetInt(ST_WorldGridPosition, worldGridPosition_INT);

            // Label Wall & Ground Distinction
            EditorGUILayout.Space(smallSpacing);
            EditorGUILayout.LabelField("Ground & Wall Distinction", EditorStyles.helpBox);
            // Display toggle Enable Distinction
            enableDistinction = EditorGUILayout.Toggle("Ground & Wall distinction", enableDistinction);
            if (enableDistinction)
            {
                enableDistinction_INT = 1;
                // Slope Tolerance
                slopeTolerance = targetMat.GetFloat(ST_Exp_Slope_Tolerance);
                slopeTolerance = EditorGUILayout.Slider("Slope Tolerance", slopeTolerance, 0f, 0.99f, GUILayout.MinWidth(180f), GUILayout.MaxWidth(3000), GUILayout.Height(20));
                targetMat.SetFloat(ST_Exp_Slope_Tolerance, slopeTolerance);
                // HelpBox Wall Grid section
                EditorGUILayout.Space(smallSpacing);
                EditorGUILayout.HelpBox("Refer to the Wall Grid section above to customize your Wall Grid", MessageType.Info);
                EditorGUILayout.Space(smallSpacing);
            }
            else
            {
                enableDistinction_INT = 0;
            }
            targetMat.SetInt(ST_EnableDistinction, enableDistinction_INT);
            
            //Spacing
            EditorGUILayout.Space(smallSpacing);
        }
        #endregion

        #region GRID
        //________SECTION - GRID
        //Header Foldout - GRID
        CoreEditorUtils.DrawSplitter();
        showingGrid = CoreEditorUtils.DrawHeaderFoldout("GRID", showingGrid, false, null);

        // Showing properties - GRID
        if (showingGrid)
        {
            // Label Grid Parameters
            EditorGUILayout.Space(smallSpacing);
            EditorGUILayout.LabelField("Grid Parameters", EditorStyles.helpBox);
            //Grid Texture
            gridTexture = targetMat.GetTexture(ST_Grid_Texture);
            gridTexture = (Texture2D)EditorGUILayout.ObjectField("Grid Texture", gridTexture, objType: typeof(Texture2D), false);
            targetMat.SetTexture(ST_Grid_Texture, gridTexture);
            EditorGUILayout.Space(smallSpacing);
            // Grid Size
            gridSize = targetMat.GetFloat(ST_GridSize);
            gridSize = EditorGUILayout.FloatField("Grid Size", gridSize);
            targetMat.SetFloat(ST_GridSize, gridSize);
            // Grid Color
            groundGridColor = targetMat.GetColor(ST_Ground_Grid_Color);
            groundGridColor = EditorGUILayout.ColorField("Grid Color", groundGridColor, GUILayout.MinWidth(180f), GUILayout.MaxWidth(3000), GUILayout.Height(20));
            targetMat.SetColor(ST_Ground_Grid_Color, groundGridColor);


            // Label Grid BackGround
            EditorGUILayout.Space(smallSpacing);
            EditorGUILayout.LabelField("BackGround Parameters", EditorStyles.helpBox);
            EditorGUILayout.Space(smallSpacing);
            // Ground BackGround Texture
            groundBackGroundTexture = targetMat.GetTexture(ST_Ground_BackGround_Texture);
            groundBackGroundTexture = (Texture2D)EditorGUILayout.ObjectField("Background Texture", groundBackGroundTexture, objType: typeof(Texture2D), false);
            targetMat.SetTexture(ST_Ground_BackGround_Texture, groundBackGroundTexture);
            // Ground BackGround Size
            groundBackGroundSize = targetMat.GetFloat(ST_Ground_BackGround_Size);
            groundBackGroundSize = EditorGUILayout.FloatField("BackGround Size", groundBackGroundSize);
            targetMat.SetFloat(ST_Ground_BackGround_Size, groundBackGroundSize);
            // Ground BackGround Color
            groundBackGroundColor = targetMat.GetColor(ST_Ground_BackGround_Color);
            groundBackGroundColor = EditorGUILayout.ColorField("BackGround Color", groundBackGroundColor, GUILayout.MinWidth(180f), GUILayout.MaxWidth(3000), GUILayout.Height(20));
            targetMat.SetColor(ST_Ground_BackGround_Color, groundBackGroundColor);

            
            // Label Grid Number
            EditorGUILayout.Space(smallSpacing);
            EditorGUILayout.LabelField("Grid Number", EditorStyles.helpBox);
            // Display Number
            // Detect current display Number
            displayNumber_INT = targetMat.GetInt(ST_Display_Number);
            if (displayNumber_INT == 1)
            {
                displayNumber = true;
            }
            else
            {
                displayNumber = false;
            }
            // Display toggle DisplayNumber
            displayNumber = EditorGUILayout.Toggle("Display Grid Number", displayNumber);
            if (displayNumber)
            {
                displayNumber_INT = 1;
                // Number Texture 
                numberTexture = targetMat.GetTexture(ST_Number_Texture);
                numberTexture = EditorGUILayout.ObjectField("Number Texture", numberTexture, typeof(Texture2D), false) as Texture2D;
                targetMat.SetTexture(ST_Number_Texture, numberTexture);
                EditorGUILayout.Space(smallSpacing);
                // Number Offset 
                numberXoffset = targetMat.GetFloat(ST_Number_Offset_X);
                numberXoffset = EditorGUILayout.Slider("Number Offset X", numberXoffset, -1, 1, GUILayout.MinWidth(180f), GUILayout.MaxWidth(3000), GUILayout.Height(20));
                targetMat.SetFloat(ST_Number_Offset_X, numberXoffset);
                numberYoffset = targetMat.GetFloat(ST_Number_Offset_Y);
                numberYoffset = EditorGUILayout.Slider("Number Offset Y", numberYoffset, -1, 1, GUILayout.MinWidth(180f), GUILayout.MaxWidth(3000), GUILayout.Height(20));
                targetMat.SetFloat(ST_Number_Offset_Y, numberYoffset);
                // Invert Grid
                // Detect current invert grid
                invertGrid_INT = targetMat.GetInt(ST_Exp_InvertGrid);
                if (invertGrid_INT == 1)
                {
                    invertGrid = true;
                }
                else
                {
                    invertGrid = false;
                }
                // Display toggle InvertGrid
                invertGrid = EditorGUILayout.Toggle("Invert Grid Number", invertGrid);
                if (invertGrid)
                {
                    invertGrid_INT = 1;
                }
                if (!invertGrid)
                {
                    invertGrid_INT = 0;
                }
                targetMat.SetInt(ST_Exp_InvertGrid, invertGrid_INT);
                // Help Box Inverted Numbers
                EditorGUILayout.Space(smallSpacing);
                EditorGUILayout.HelpBox("Some of the numbers displayed are inverted because of the way the shader is built, this option does not solve this problem but allows to change the numbers that are visually inverted.", MessageType.Info);
                EditorGUILayout.Space(smallSpacing);
            }
            else
            {
                displayNumber_INT = 0;
            }
            targetMat.SetInt(ST_Display_Number, displayNumber_INT);

            // Label Grid Emssion
            EditorGUILayout.Space(smallSpacing);
            EditorGUILayout.LabelField("Color Options", EditorStyles.helpBox);
            // BackGround Intensity
            backgroundIntensity = targetMat.GetFloat(ST_BackGround_Intensity);
            backgroundIntensity = EditorGUILayout.Slider("BackGround Intensity", backgroundIntensity, 0, 3, GUILayout.MinWidth(180f), GUILayout.MaxWidth(3000), GUILayout.Height(20));
            targetMat.SetFloat(ST_BackGround_Intensity, backgroundIntensity);
            //Emissive Bool
            // Detect current State of Emissive Grid
            emissiveGrid_INT = targetMat.GetInt(ST_Emissive_Grid);
            emissivePower = targetMat.GetFloat(ST_Emissive_Power);
            if (emissiveGrid_INT == 1)
            {
                emissiveGrid = true;
            }
            else
            {
                emissiveGrid = false;
            }
            // Display toggle Emissive Grid
            emissiveGrid = EditorGUILayout.Toggle("Emissive Grid", emissiveGrid, GUILayout.MinWidth(180f), GUILayout.MaxWidth(3000), GUILayout.Height(20));
            if (emissiveGrid)
            {
                emissiveGrid_INT = 1;
                //Emissive Power
                emissivePower = EditorGUILayout.Slider("Emissive Power", emissivePower, 0, 3, GUILayout.MinWidth(180f), GUILayout.MaxWidth(3000), GUILayout.Height(20));
            }
            if (!emissiveGrid)
            {
                emissiveGrid_INT = 0;
            }
            targetMat.SetInt(ST_Emissive_Grid, emissiveGrid_INT);
            targetMat.SetFloat(ST_Emissive_Power, emissivePower);


            //Spacing
            EditorGUILayout.Space(smallSpacing);
        }
        else
        {

        }
        #endregion

        #region WALL GRID
        //________SECTION - WALL GRIDD
        //Header Foldout - WALL GRID
        if (enableDistinction)
        {
            CoreEditorUtils.DrawSplitter();
            showingWallGrid = CoreEditorUtils.DrawHeaderFoldout("WALL GRID", showingWallGrid, false, null);

            // Showing Properties - Wall Grid
            if (showingWallGrid)
            {

                //Label Wall BackGround
                EditorGUILayout.Space(smallSpacing);
                EditorGUILayout.LabelField("Wall Grid", EditorStyles.helpBox);
                EditorGUILayout.Space(smallSpacing);
                //Wall Grid Color
                wallGridColor = targetMat.GetColor(ST_Wall_Grid_Color);
                wallGridColor = EditorGUILayout.ColorField("Wall Grid Color", wallGridColor, GUILayout.MinWidth(180f), GUILayout.MaxWidth(3000), GUILayout.Height(20));
                targetMat.SetColor(ST_Wall_Grid_Color, wallGridColor);
                //Label Wall BackGround
                EditorGUILayout.Space(smallSpacing);
                EditorGUILayout.LabelField("Wall BackGround", EditorStyles.helpBox);
                EditorGUILayout.Space(smallSpacing);
                //Wall BackGround Texture
                wallBackGroundTexture = targetMat.GetTexture(ST_Wall_BackGround_Texture);
                wallBackGroundTexture = (Texture2D)EditorGUILayout.ObjectField("Wall Background Texture", wallBackGroundTexture, objType: typeof(Texture2D), false);
                targetMat.SetTexture(ST_Wall_BackGround_Texture, wallBackGroundTexture);
                //Wall BackGround Size
                wallBackGroundSize = targetMat.GetFloat(ST_Wall_BackGround_Size);
                wallBackGroundSize = EditorGUILayout.FloatField("Wall BackGround Size", wallBackGroundSize);
                targetMat.SetFloat(ST_Wall_BackGround_Size, wallBackGroundSize);
                //Wall BackGround Color
                wallBackGroundColor = targetMat.GetColor(ST_Wall_BackGround_Color);
                wallBackGroundColor = EditorGUILayout.ColorField("Wall BackGround Color", wallBackGroundColor, GUILayout.MinWidth(180f), GUILayout.MaxWidth(3000), GUILayout.Height(20));
                targetMat.SetColor(ST_Wall_BackGround_Color, wallBackGroundColor);

                //Spacing
                EditorGUILayout.Space(smallSpacing);

            }
            else
            {

            }
        }        
        #endregion

        #region DEBUG
        //________SECTION - DEBUG
        //Header Foldout - Debug
        CoreEditorUtils.DrawSplitter();
        showingDebug = CoreEditorUtils.DrawHeaderFoldout("DEBUG", showingDebug, false, null);

        // Showing Properties - Debug
        if (showingDebug)
        {
            // HelpBox warning
            EditorGUILayout.Space(smallSpacing);
            EditorGUILayout.HelpBox("This part is the basic material editor, use it only if you have a bug to check the actual properties of the shader. Changing the properties here can cause unwanted issues.", MessageType.Warning);
            EditorGUILayout.Space(smallSpacing);

            // Showing Basic Metrial Editor
            showingBasic = EditorGUILayout.Toggle("Display basic Material Editor?", showingBasic);
            if (showingBasic)
            {
                materialEditor.PropertiesDefaultGUI(properties);
            }
        }
        else
        {

        }
        // SERIALISATION & UNDO
        Undo.RecordObject(targetMat, "Target Mat");
        EditorUtility.SetDirty(targetMat);
        #endregion
    }
}
#endif
