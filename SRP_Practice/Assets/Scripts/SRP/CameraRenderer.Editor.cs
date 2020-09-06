#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace Pedev {
    partial class CameraRenderer {
        partial void DrawUnsupportedShaders();
        partial void DrawGizmos();
        partial void PrepareUIGeometryForSceneWindow();

        partial void PrepareBuffer();

#if UNITY_EDITOR
        static ShaderTagId[] legacyShaderTagIds = {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM")
        };
        static Material errorMaterial;

        partial void DrawUnsupportedShaders() {
            if (errorMaterial == null) {
                errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
            }
            var filteringSettings = FilteringSettings.defaultValue;
            var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera)){
                overrideMaterial = errorMaterial
            };
            for (int i = 1; i < legacyShaderTagIds.Length; i++) {
                drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
            }
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        }

        partial void DrawGizmos() {
            if (Handles.ShouldRenderGizmos()) {
                context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
        }

        partial void PrepareUIGeometryForSceneWindow() {
            if (camera.cameraType == CameraType.SceneView) {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
        }

        partial void PrepareBuffer() {
            cameraBuffer.name = camera.name;
        }
#endif
    }
}
