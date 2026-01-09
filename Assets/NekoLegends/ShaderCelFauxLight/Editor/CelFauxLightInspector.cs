
#if UNITY_EDITOR
using UnityEditor;

namespace NekoLegends
{
    public class CelFauxLightInspector : ShaderGUIBase
    {

        protected bool showNormalProperties,showLightingProperties;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            ShowLogo();
            ShowMainSection(materialEditor, properties);
            ShowLightingSection(materialEditor, properties);
            ShowNormalSection(materialEditor, properties);
        }

        protected void ShowMainSection(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            MaterialProperty _Main_Texture = FindProperty("_Main_Texture", properties);
            MaterialProperty _Color = FindProperty("_Color", properties);
            MaterialProperty _ShadingFalloff = FindProperty("_ShadingFalloff", properties);
            MaterialProperty _Brightness = FindProperty("_Brightness", properties);

            materialEditor.ShaderProperty(_Main_Texture, _Main_Texture.displayName);
            materialEditor.ShaderProperty(_Color, _Color.displayName);
            materialEditor.ShaderProperty(_ShadingFalloff, _ShadingFalloff.displayName);
            materialEditor.ShaderProperty(_Brightness, _Brightness.displayName);

        }

        protected virtual void ShowNormalSection(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            MaterialProperty _NormalTex = FindProperty("_NormalTex", properties);
            MaterialProperty _NormalIntensity = FindProperty("_NormalIntensity", properties);

            showNormalProperties = EditorPrefs.GetBool("CelFauxLightInspector_showNormalProperties", false);
            showNormalProperties = EditorGUILayout.BeginFoldoutHeaderGroup(showNormalProperties, "Normal");
            if (showNormalProperties)
            {
                materialEditor.ShaderProperty(_NormalTex, _NormalTex.displayName);
                materialEditor.ShaderProperty(_NormalIntensity,_NormalIntensity.displayName);

            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorPrefs.SetBool("CelFauxLightInspector_showNormalProperties", showNormalProperties);
        }

        protected virtual void ShowLightingSection(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            MaterialProperty _RimOutput = FindProperty("_RimOutput", properties);
            MaterialProperty _RimColor = FindProperty("_RimColor", properties);

            MaterialProperty _UseEmission = FindProperty("_UseEmission", properties);
            MaterialProperty _EmissionTex = FindProperty("_EmissionTex", properties);
            MaterialProperty _EmissionColor = FindProperty("_EmissionColor", properties);
            MaterialProperty _EmissionIntensity = FindProperty("_EmissionIntensity", properties);

            MaterialProperty _LightAzimuth = FindProperty("_LightAzimuth", properties);
            MaterialProperty _LightElevation = FindProperty("_LightElevation", properties);

            


            showLightingProperties = EditorPrefs.GetBool("CelFauxLightInspector_showLightingProperties", false);
            showLightingProperties = EditorGUILayout.BeginFoldoutHeaderGroup(showLightingProperties, "Lighting");
            if (showLightingProperties)
            {

                materialEditor.ShaderProperty(_LightAzimuth, _LightAzimuth.displayName);
                materialEditor.ShaderProperty(_LightElevation, _LightElevation.displayName);

                materialEditor.ShaderProperty(_RimOutput, _RimOutput.displayName);
                materialEditor.ShaderProperty(_RimColor, _RimColor.displayName);

                // Display _UseEmission as a checkbox
                bool useEmission = EditorGUILayout.Toggle("Use Emission", _UseEmission.floatValue == 1.0f);
                _UseEmission.floatValue = useEmission ? 1.0f : 0.0f;

                // If _UseEmission is checked (true), then display the emission properties
                if (_UseEmission.floatValue == 1.0f)
                {
                    materialEditor.ShaderProperty(_EmissionTex, _EmissionTex.displayName);
                    materialEditor.ShaderProperty(_EmissionColor, _EmissionColor.displayName);
                    materialEditor.ShaderProperty(_EmissionIntensity, _EmissionIntensity.displayName);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorPrefs.SetBool("CelFauxLightInspector_showLightingProperties", showLightingProperties);
        }



    }

}

#endif
