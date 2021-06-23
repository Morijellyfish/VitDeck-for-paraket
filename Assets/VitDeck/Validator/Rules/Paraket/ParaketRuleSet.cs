// 当ファイルは Assets/VitDeck/Validator/Rules/Vket/VketRuleSetBase.cs、および ConceptWorldRuleSet.cs をコピーして改変したもの

using UnityEngine;
using VitDeck.Language;

#if VRC_SDK_VRCSDK3
using VRC.SDKBase;
#elif VRC_SDK_VRCSDK2
using VRCSDK2;
#endif

namespace VitDeck.Validator
{

    /// <summary>
    /// パラレルマーケットのルールセット。
    /// </summary>
    public class ParaketRuleSet : IRuleSet
    {
        public string RuleSetName => "パラレルマーケット";

        protected readonly long MegaByte = 1048576;

        private readonly VketTargetFinder targetFinder = new VketTargetFinder();
        public IValidationTargetFinder TargetFinder => targetFinder;

        private readonly IObjectDetector officialPrefabsDetector;

        public ParaketRuleSet() : base()
        {
            officialPrefabsDetector = new PrefabPartsDetector(
                VketOfficialAssetData.AudioSourcePrefabGUIDs,
                VketOfficialAssetData.AvatarPedestalPrefabGUIDs,
                VketOfficialAssetData.ChairPrefabGUIDs,
                VketOfficialAssetData.PickupObjectSyncPrefabGUIDs,
                VketOfficialAssetData.CanvasPrefabGUIDs,
                VketOfficialAssetData.PointLightProbeGUIDs);
        }

        public IRule[] GetRules()
        {
            // デフォルトで使っていたAttribute式は宣言時にconst以外のメンバーが利用できない。
            // 継承したプロパティを参照して挙動を変えることが出来ない為、直接リストを返す方式に変更した。
            return new IRule[]
            {
#if VRC_SDK_VRCSDK2

                //A-1 バージョン制限
                new UnityVersionRule(LocalizedMessage.Get("VketRuleSetBase.UnityVersionRule.Title", "2018.4.20f1"), "2018.4.20f1"),
                //A-2 SDKバージョン制限
                new VRCSDKVersionRule(LocalizedMessage.Get("VketRuleSetBase.VRCSDKVersionRule.Title"),
                    new VRCSDKVersion("2020.05.06.12.14"),
                    "https://files.vrchat.cloud/sdk/VRCSDK2-2020.09.15.11.25_Public.unitypackage"),
                //F Scene構造 Dyanamic-Static
                new ExistInSubmitFolderRule(LocalizedMessage.Get("VketRuleSetBase.ExistInSubmitFolderRule.Title"), VketOfficialAssetData.GUIDs, targetFinder),
                //Blacklist
                new AssetGuidBlacklistRule(LocalizedMessage.Get("VketRuleSetBase.OfficialAssetDontContainRule.Title"), VketOfficialAssetData.GUIDs),
                //B-3 命名規則
                new AssetNamingRule(LocalizedMessage.Get("VketRuleSetBase.NameOfFileAndFolderRule.Title"), @"[a-zA-Z0-9 _\.\-\(\)]+"),
                //Vket B-3 パスの長さ制限
                new AssetPathLengthRule(LocalizedMessage.Get("VketRuleSetBase.FilePathLengthLimitRule.Title", 184), 184),
                //B-4 拡張子制限
                new AssetExtentionBlacklistRule(LocalizedMessage.Get("VketRuleSetBase.MeshFileTypeBlacklistRule.Title"),
                                                new string[]{".ma", ".mb", "max", "c4d", ".blend"}
                ),
                //MaterialとTextureの検証
                new ContainMatOrTexInAssetRule(LocalizedMessage.Get("VketRuleSetBase.ContainMatOrTexInAssetRule.Title")),
                //B-2 Unitypackageのサイズ制限
                new FolderSizeRule(LocalizedMessage.Get("VketRuleSetBase.FolderSizeRule.Title"), 50 * MegaByte),
                //ブース内の構造
                new ExhibitStructureRule(LocalizedMessage.Get("VketRuleSetBase.ExhibitStructureRule.Title")),
                //Staticフラグの管理
                new StaticFlagRule(LocalizedMessage.Get("VketRuleSetBase.StaticFlagsRule.Title")),
                //C-1 ブースの大きさ
                new BoothBoundsRule(LocalizedMessage.Get("VketRuleSetBase.BoothBoundsRule.Title"),
                    size: new Vector3(4, 5, 4),
                    margin: 0.01f),
                //D-1 Material制限
                new AssetTypeLimitRule(
                    LocalizedMessage.Get("VketRuleSetBase.MaterialLimitRule.Title", 20),
                    typeof(Material),
                    20,
                    VketOfficialAssetData.MaterialGUIDs),
                //D-5 ライトマップ制限
                new LightmapSizeLimitRule(
                    LocalizedMessage.Get("VketRuleSetBase.LightMapsLimitRule.Title", 1, 512),
                    lightmapCountLimit: 1,
                    lightmapResolutionLimit: 512),
                //E-1 Bakedのみ許可
                new GlobalIlluminationBakedRule(LocalizedMessage.Get("VketRuleSetBase.GlobalIlluminationBakedRule.Title")),
                //G 使用可能Component 後述
                new UsableComponentListRule(LocalizedMessage.Get("VketRuleSetBase.UsableComponentListRule.Title"),
                    GetComponentReferences(),
                    ignorePrefabGUIDs: VketOfficialAssetData.GUIDs),
                //G SkinnedMeshの規則
                new SkinnedMeshRendererRule(LocalizedMessage.Get("VketRuleSetBase.SkinnedMeshRendererRule.Title")),
                //MeshRenderer
                new MeshRendererRule(LocalizedMessage.Get("VketRuleSetBase.MeshRendererRule.Title")),
                //RefrectionProbeの設定 Paraketでは不可
                new ReflectionProbeRule(LocalizedMessage.Get("VketRuleSetBase.ReflectionProbeRule.Title")),
                //G Trigger制限
                new VRCTriggerConfigRule(LocalizedMessage.Get("VketRuleSetBase.VRCTriggerConfigRule.Title"),
                            VRCTriggerBroadcastTypesWhitelist,
                            new VRC_Trigger.TriggerType[] {
                                VRC_Trigger.TriggerType.Custom,
                                VRC_Trigger.TriggerType.OnInteract,
                                VRC_Trigger.TriggerType.OnEnterTrigger,
                                VRC_Trigger.TriggerType.OnExitTrigger,
                                VRC_Trigger.TriggerType.OnPickup,
                                VRC_Trigger.TriggerType.OnDrop,
                                VRC_Trigger.TriggerType.OnPickupUseDown,
                                VRC_Trigger.TriggerType.OnPickupUseUp   },
                            VRCTriggerActionWhitelist,
                            VketOfficialAssetData.GUIDs),
                //? MeshCollider禁止
                new UseMeshColliderRule(LocalizedMessage.Get("VketRuleSetBase.UseMeshColliderRule.Title")),
                //G トリガー個数制限
                new VRCTriggerCountLimitRule(LocalizedMessage.Get("VketRuleSetBase.VRCTriggerCountLimitRule.Title", 25), 25),

                //G 以下 ライト制限
                //Directional count (0)
                new LightCountLimitRule(LocalizedMessage.Get("VketRuleSetBase.DirectionalLightLimitRule.Title"), UnityEngine.LightType.Directional, 0),
                //Point
                new LightConfigRule(LocalizedMessage.Get("VketRuleSetBase.PointLightConfigRule.Title"), UnityEngine.LightType.Point, new LightConfigRule.LightConfig(
                            new[] { LightmapBakeType.Baked, LightmapBakeType.Realtime },
                            0, 7,
                            0, 10,
                            0, 15)),
                //Spot
                new LightConfigRule(LocalizedMessage.Get("VketRuleSetBase.SpotLightConfigRule.Title"), UnityEngine.LightType.Spot, new LightConfigRule.LightConfig(
                            new[] { LightmapBakeType.Baked, LightmapBakeType.Realtime },
                            0, 7,
                            0, 10,
                            0, 15)),
                //Area
                new LightConfigRule(LocalizedMessage.Get("VketRuleSetBase.AreaLightConfigRule.Title"), UnityEngine.LightType.Area, new LightConfigRule.LightConfig(
                            new[] { LightmapBakeType.Baked },
                            0, 30,
                            0, 10,
                            0, 15)),
                //Area count
                new LightCountLimitRule(
                    LocalizedMessage.Get("VketRuleSetBase.AreaLightLimitRule.Title", 3),
                    UnityEngine.LightType.Area,
                    3),
                //AnimationとColliderに関する制限
                new AnimationMakesMoveCollidersRule(LocalizedMessage.Get("VketRuleSetBase.AnimationMakesMoveCollidersRule.Title")),
                //Animation Material変更制限 Path制限
                new AnimationClipRule(LocalizedMessage.Get("VketRuleSetBase.AnimationClipRule.Title")),
                //Animation Alwaysの禁止
                new AnimationComponentRule(LocalizedMessage.Get("VketRuleSetBase.AnimationComponentRule.Title"), officialPrefabsDetector),
                //Rootmotion 使用禁止
                new AnimatorComponentRule(LocalizedMessage.Get("VketRuleSetBase.AnimatorComponentRule.Title"),
                    new System.Type[]{
                        typeof(VRC_Pickup),
                        typeof(VRC_ObjectSync)
                    },officialPrefabsDetector),
                //G CanvasRenderはWorldSpaceのみ
                new CanvasRenderModeRule(LocalizedMessage.Get("VketRuleSetBase.CanvasRenderModeRule.Title")),
                //Canvasサイズ確認
                new CameraComponentRule(LocalizedMessage.Get("VketRuleSetBase.CameraComponentRule.Title"), maxRenderTextureSize: new Vector2(1024, 1024)),
                //CameraComponent制限
                new CameraComponentMaxCountRule(LocalizedMessage.Get("VketRuleSetBase.CameraComponentMaxCountRule.Title"), limit: 1),
                //ProjectorComponent制限
                new ProjectorComponentRule(LocalizedMessage.Get("VketRuleSetBase.ProjectorComponentRule.Title")),
                //ProjectorComponent個数制限
                new ProjectorComponentMaxCountRule(LocalizedMessage.Get("VketRuleSetBase.ProjectorComponentMaxCountRule.Title"), limit: 1),
                //PickupObjectの制限
                new PickupObjectSyncPrefabRule(LocalizedMessage.Get("VketRuleSetBase.PickupObjectSyncRule.Title"), VketOfficialAssetData.PickupObjectSyncPrefabGUIDs),
                //G AvatarPedestal制限
                new AvatarPedestalPrefabRule(LocalizedMessage.Get("VketRuleSetBase.AvatarPedestalPrefabRule.Title"), VketOfficialAssetData.AvatarPedestalPrefabGUIDs),
                //G AudioSource制限
                new AudioSourcePrefabRule(LocalizedMessage.Get("VketRuleSetBase.AudioSourcePrefabRule.Title"),  VketOfficialAssetData.AudioSourcePrefabGUIDs),
                //G RigidBody制限
                new RigidbodyRule(LocalizedMessage.Get("VketRuleSetBase.RigidbodyRule.Title")),
                //G 椅子の個数制限
                new PrefabLimitRule(
                    LocalizedMessage.Get("VketRuleSetBase.ChairPrefabLimitRule.Title", 1),
                    VketOfficialAssetData.ChairPrefabGUIDs,
                    1),
                //許可されていないprefabの設置制限
                new PrefabLimitRule(
                    LocalizedMessage.Get("VketRuleSetBase.UnusabePrefabRule.Title", 1),
                    VketOfficialAssetData.VRCSDKPrefabGUIDs,
                    0),
                //G Pirckupの制限
                new PrefabLimitRule(
                    LocalizedMessage.Get("VketRuleSetBase.PickupObjectSyncPrefabLimitRule.Title", 5),
                    VketOfficialAssetData.PickupObjectSyncPrefabGUIDs,
                    5),
                //VideoPlayerの機能制限
                new VideoPlayerComponentRule(LocalizedMessage.Get("VketRuleSetBase.VideoPlayerComponentRule.Title")),
                //VideoPlayerの個数制限
                new VideoPlayerComponentMaxCountRule(LocalizedMessage.Get("VketRuleSetBase.VideoPlayerComponentMaxCountRule.Title"), limit: 1),
                //G Animationの個数制限
                new AnimatorComponentMaxCountRule(LocalizedMessage.Get("VketRuleSetBase.AnimatorComponentMaxCountRule.Title"), limit: 50)
#endif
            };
        }

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3 
        //ブロードキャストタイプをlocalのみに限定
        protected virtual VRC_EventHandler.VrcBroadcastType[] VRCTriggerBroadcastTypesWhitelist
        {
            get
            {
                return new VRC_EventHandler.VrcBroadcastType[]{
                    VRC_EventHandler.VrcBroadcastType.Local };
            }
        }
        //使用可能Trigger
        protected virtual VRC_EventHandler.VrcEventType[] VRCTriggerActionWhitelist
        {
            get
            {
                return new VRC_EventHandler.VrcEventType[] {
                    VRC_EventHandler.VrcEventType.ActivateCustomTrigger,
                    VRC_EventHandler.VrcEventType.AudioTrigger,
                    VRC_EventHandler.VrcEventType.PlayAnimation,
                    VRC_EventHandler.VrcEventType.SetParticlePlaying,
                    VRC_EventHandler.VrcEventType.SetComponentActive,
                    VRC_EventHandler.VrcEventType.SetGameObjectActive,
                    VRC_EventHandler.VrcEventType.AnimationBool,
                    VRC_EventHandler.VrcEventType.AnimationFloat,
                    VRC_EventHandler.VrcEventType.AnimationInt,
                    VRC_EventHandler.VrcEventType.AnimationIntAdd,
                    VRC_EventHandler.VrcEventType.AnimationIntDivide,
                    VRC_EventHandler.VrcEventType.AnimationIntMultiply,
                    VRC_EventHandler.VrcEventType.AnimationIntSubtract,
                    VRC_EventHandler.VrcEventType.AnimationTrigger};
            }
        }
#endif
        //使用可能Component
        private ComponentReference[] GetComponentReferences()
        {
            return new ComponentReference[] {
                new ComponentReference("VRC_Trigger", new string[]{"VRCSDK2.VRC_Trigger", "VRCSDK2.VRC_EventHandler"}, AdvancedObjectValidationLevel),
                new ComponentReference("VRC_Object Sync", new string[]{"VRCSDK2.VRC_ObjectSync"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Pickup", new string[]{"VRCSDK2.VRC_Pickup"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Audio Bank", new string[]{"VRCSDK2.VRC_AudioBank"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Avatar Pedestal", new string[]{"VRCSDK2.VRC_AvatarPedestal"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Ui Shape", new string[]{"VRCSDK2.VRC_UiShape"}, AdvancedObjectValidationLevel),
                new ComponentReference("Rigidbody", new string[]{"UnityEngine.Rigidbody"}, ValidationLevel.ALLOW),
                new ComponentReference("Cloth", new string[]{"UnityEngine.Cloth"}, MoreAdvancedObjectValidationLevel),
                new ComponentReference("Collider", new string[]{"UnityEngine.SphereCollider", "UnityEngine.BoxCollider", "UnityEngine.SphereCollider", "UnityEngine.CapsuleCollider", "UnityEngine.MeshCollider", "UnityEngine.WheelCollider"}, ValidationLevel.ALLOW),
                new ComponentReference("Dynamic Bone", new string[]{"DynamicBone"}, ValidationLevel.ALLOW),
                new ComponentReference("Dynamic Bone Collider", new string[]{"DynamicBoneCollider"}, ValidationLevel.ALLOW),
                new ComponentReference("Skinned Mesh Renderer", new string[]{"UnityEngine.SkinnedMeshRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("Mesh Renderer ", new string[]{"UnityEngine.MeshRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("Mesh Filter", new string[]{"UnityEngine.MeshFilter"}, ValidationLevel.ALLOW),
                new ComponentReference("Particle System", new string[]{"UnityEngine.ParticleSystem", "UnityEngine.ParticleSystemRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("Trail Renderer", new string[]{"UnityEngine.TrailRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("Line Renderer", new string[]{"UnityEngine.LineRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("Light", new string[]{"UnityEngine.Light"}, AdvancedObjectValidationLevel),
                new ComponentReference("LightProbeGroup", new string[]{"UnityEngine.LightProbeGroup"}, AdvancedObjectValidationLevel),
                new ComponentReference("ReflectionProbe", new string[]{"UnityEngine.ReflectionProbe"}, AdvancedObjectValidationLevel),
                new ComponentReference("Camera", new string[]{"UnityEngine.Camera"}, MoreAdvancedObjectValidationLevel),
                new ComponentReference("Projector", new string[]{"UnityEngine.Projector"}, MoreAdvancedObjectValidationLevel),
                new ComponentReference("LookatTarget", new string[]{"UnityStandardAssets.Cameras.LookatTarget" }, MoreAdvancedObjectValidationLevel),
                new ComponentReference("FollowTarget", new string[]{"UnityStandardAssets.Utility.FollowTarget" }, MoreAdvancedObjectValidationLevel),
                new ComponentReference("Suspension", new string[]{"UnityStandardAssets.Vehicles.Car.Suspension" }, MoreAdvancedObjectValidationLevel),
                new ComponentReference("Animator", new string[]{"UnityEngine.Animator"}, ValidationLevel.ALLOW),
                new ComponentReference("Animation", new string[]{"UnityEngine.Animation"}, ValidationLevel.ALLOW),
                new ComponentReference("Audio Source", new string[]{"UnityEngine.AudioSource", "ONSPAudioSource", "VRCSDK2.VRC_SpatialAudioSource"}, ValidationLevel.DISALLOW),
                new ComponentReference("Canvas", new string[]{"UnityEngine.Canvas", "UnityEngine.CanvasGroup", "UnityEngine.RectTransform", "UnityEngine.UI.CanvasScaler", "UnityEngine.UI.GraphicRaycaster", "UnityEngine.UI.AspectRatioFitter", "UnityEngine.UI.LayoutElement", "UnityEngine.UI.ContentSizeFitter", "UnityEngine.UI.HorizontalLayoutGroup", "UnityEngine.UI.VerticalLayoutGroup", "UnityEngine.UI.GridLayoutGroup", "UnityEngine.UI.Text", "UnityEngine.UI.Image", "UnityEngine.UI.RawImage", "UnityEngine.UI.Mask", "UnityEngine.UI.RectMask2D", "UnityEngine.UI.Button", "UnityEngine.UI.InputField", "UnityEngine.UI.Toggle", "UnityEngine.UI.ToggleGroup", "UnityEngine.UI.Slider", "UnityEngine.UI.Scrollbar", "UnityEngine.UI.Dropdown", "UnityEngine.UI.ScrollRect", "UnityEngine.UI.Selectable", "UnityEngine.UI.Shadow", "UnityEngine.UI.Outline", "UnityEngine.UI.PositionAsUV1", "UnityEngine.RectTransform", "UnityEngine.CanvasRenderer"}, ValidationLevel.ALLOW),
                new ComponentReference("VideoPlayer", new string[]{"UnityEngine.Video.VideoPlayer" }, MoreAdvancedObjectValidationLevel),
                new ComponentReference("VRC_Station", new string[]{"VRCSDK2.VRC_Station"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Mirror", new string[]{ "VRCSDK2.VRC_MirrorCamera", "VRCSDK2.VRC_MirrorReflection" }, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_PlayerAudioOverride", new string[]{"VRCSDK2.VRC_PlayerAudioOverride"}, ValidationLevel.DISALLOW),
                new ComponentReference("EventSystem", new string[]{"UnityEngine.EventSystems.EventSystem", "UnityEngine.EventSystems.StandaloneInputModule"}, ValidationLevel.DISALLOW),
                new ComponentReference("StandaloneInputModule", new string[]{"UnityEngine.EventSystems.StandaloneInputModule"}, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_SceneResetPosition", new string[]{"VRCSDK2.VRC_SceneResetPosition"}, ValidationLevel.DISALLOW),
                new ComponentReference("PlayableDirector", new string[]{"UnityEngine.Playables.PlayableDirector" }, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_Panorama", new string[]{"VRCSDK2.scripts.Scenes.VRC_Panorama" }, ValidationLevel.DISALLOW),
                new ComponentReference("VRC_SyncVideoPlayer", new string[]{"VRCSDK2.VRC_SyncVideoPlayer", "VRCSDK2.VRC_SyncVideoStream" }, ValidationLevel.DISALLOW),
            };
        }

        protected virtual ValidationLevel AdvancedObjectValidationLevel
        {
            get
            {
                return ValidationLevel.ALLOW;
            }
        }

        protected virtual ValidationLevel MoreAdvancedObjectValidationLevel
        {
            get
            {
                return ValidationLevel.DISALLOW;
            }
        }
    }
}
