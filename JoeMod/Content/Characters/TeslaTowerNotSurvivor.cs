﻿using BepInEx.Configuration;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;
using ModdedEntityStates.TeslaTrooper;
using Modules.Characters;
using ModdedEntityStates.TeslaTrooper.Tower;
using EntityStates;
using R2API;
using RoR2.CharacterAI;

namespace Modules.Survivors {
    internal class TeslaTowerNotSurvivor : CharacterBase {

        public override string bodyName => "TeslaTower";

        public const string TOWER_PREFIX = FacelessJoePlugin.DEV_PREFIX + "_TESLA_TOWER_BODY_";

        public override BodyInfo bodyInfo { get; set; } = new BodyInfo {
            armor = 150f,
            armorGrowth = 0f,
            bodyName = "TeslaTowerBody",
            bodyNameToken = TOWER_PREFIX + "NAME",
            subtitleNameToken = FacelessJoePlugin.DEV_PREFIX + "_TESLA_TOWER_BODY_SUBTITLE",
            bodyNameToClone = "EngiTurret",
            bodyColor = new Color(134f / 216f, 234f / 255f, 255f / 255f), //new Color(115f/216f, 216f/255f, 0.93f),
            characterPortrait = Modules.Assets.LoadCharacterIcon("texIconTeslaTower"),
            crosshair = Modules.Assets.LoadCrosshair("TiltedBracket"),
            podPrefab = null,
            
            maxHealth = 200f,
            healthRegen = 0f,
            damage = 7f,
            moveSpeed = 0,
            jumpCount = 0,
            
            //todo camera stuck in tower when you play as it
            aimOriginPosition = new Vector3( 0, 10, 0),
            cameraPivotPosition = new Vector3(0, 9, 0),
            cameraParamsVerticalOffset = 20,
            cameraParamsDepth= -20
        };

        public override CustomRendererInfo[] customRendererInfos { get; set; }
        
        public override Type characterMainState => typeof(TowerIdleSearch);

        public override Type characterSpawnState => typeof(TowerSpawnState);

        public override ItemDisplaysBase itemDisplays => new TeslaTowerItemDisplays();

        public static GameObject masterPrefab;
        internal static SkinDef.MinionSkinReplacement MCMinionSkinReplacement;

        public override void InitializeCharacter() {
            base.InitializeCharacter();

            bodyPrefab.AddComponent<TowerWeaponComponent>();
            bodyPrefab.AddComponent<TowerOwnerTrackerComponent>();
        }

        protected override void InitializeCharacterBodyAndModel() {
            base.InitializeCharacterBodyAndModel();

            bodyPrefab.GetComponent<CharacterBody>().overrideCoreTransform = bodyCharacterModel.GetComponent<ChildLocator>().FindChild("Head");

            bodyPrefab.GetComponent<SfxLocator>().deathSound = "Play_building_uselbuil";
            bodyPrefab.GetComponent<SfxLocator>().aliveLoopStart = ""; //todo sfx

            UnityEngine.Object.Destroy(bodyPrefab.GetComponent<SetStateOnHurt>());
            UnityEngine.Object.Destroy(bodyPrefab.GetComponent<AkEvent>());

            bodyCharacterModel.GetComponent<ChildLocator>().FindChild("LightningParticles").GetComponent<ParticleSystemRenderer>().material = Assets.ChainLightningMaterial;
        }

        protected override void InitializeEntityStateMachine() {
            base.InitializeEntityStateMachine();

            //UnityEngine.Object.Destroy(EntityStateMachine.FindByCustomName(bodyPrefab, "Weapon"));
            //Array.Resize(ref bodyPrefab.GetComponent<NetworkStateMachine>().stateMachines, 1);
            //Array.Resize(ref bodyPrefab.GetComponent<CharacterDeathBehavior>().idleStateMachine, 0);
            EntityStateMachine entityStateMachine = EntityStateMachine.FindByCustomName(bodyPrefab, "Weapon");
            entityStateMachine.initialStateType = new SerializableEntityStateType(typeof(TowerLifetime));
            entityStateMachine.mainStateType = new SerializableEntityStateType(typeof(TowerLifetime));
            States.entityStates.Add(typeof(TowerLifetime));

            bodyPrefab.GetComponent<CharacterDeathBehavior>().deathState = new SerializableEntityStateType(typeof(TowerSell));
            States.entityStates.Add(typeof(TowerSell));
        }

        protected override void InitializeCharacterMaster() {
            base.InitializeCharacterMaster();

            masterPrefab = PrefabAPI.InstantiateClone(Assets.LoadAsset<GameObject>("Prefabs/CharacterMasters/EngiTurretMaster"), "TeslaTowerMaster", true);
            masterPrefab.GetComponent<CharacterMaster>().bodyPrefab = bodyPrefab;

            foreach (AISkillDriver aiSkillDriver in masterPrefab.GetComponents<AISkillDriver>()) {
                UnityEngine.Object.Destroy(aiSkillDriver);
                //todo: proper ai?
            }
            masterPrefab.GetComponent<BaseAI>().skillDrivers = new AISkillDriver[0];

            Modules.Content.AddMasterPrefab(masterPrefab);
        }

        #region skills

        public override void InitializeSkills() {          //maybe least elegant of my solutions but it's a DRY fix so half and half
            Modules.Skills.CreateSkillFamilies(bodyPrefab, 3);

            InitializePrimarySkills();

            InitializeSecondarySkills();
        }

        private void InitializePrimarySkills() {
            States.entityStates.Add(typeof(TowerZap));
            SkillDef primarySkillDefZap = Modules.Skills.CreateSkillDef(new SkillDefInfo("Tower_Primary_Zap",
                                                                                         TOWER_PREFIX + "PRIMARY_ZAP_NAME",
                                                                                         TOWER_PREFIX + "PRIMARY_ZAP_DESCRIPTION",
                                                                                         Modules.Assets.LoadAsset<Sprite>("texIconTeslaTower"),
                                                                                         new EntityStates.SerializableEntityStateType(typeof(TowerZap)),
                                                                                         "Body",
                                                                                         false));

            Modules.Skills.AddPrimarySkills(bodyPrefab, primarySkillDefZap);
        }

        private void InitializeSecondarySkills() {
            States.entityStates.Add(typeof(TowerBigZap));
            SkillDef bigZapSkillDef = Modules.Skills.CreateSkillDef(new SkillDefInfo {
                skillName = "Tower_Secondary_BigZap",
                skillNameToken = TOWER_PREFIX + "SECONDARY_BIGZAP_NAME",
                skillDescriptionToken = TOWER_PREFIX + "SECONDARY_BIGZAP_DESCRIPTION",
                skillIcon = Modules.Assets.LoadAsset<Sprite>("texTeslaSkillSecondaryThunderclap"),
                activationState = new EntityStates.SerializableEntityStateType(typeof(TowerBigZap)),
                activationStateMachineName = "Body",
                baseMaxStock = 1,
                baseRechargeInterval = 9f,
                beginSkillCooldownOnSkillEnd = true,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Skill,
                resetCooldownTimerOnUse = false,
                isCombatSkill = true,
                mustKeyPress = false,
                cancelSprintingOnActivation = true,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,
                keywordTokens = new string[] { "KEYWORD_SHOCKING" }
            });

            Modules.Skills.AddSecondarySkills(bodyPrefab, bigZapSkillDef);
        }
#endregion skills

        public override void InitializeSkins() {
            GameObject model = bodyPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin

            SkinDef defaultSkin = Modules.Skins.CreateSkinDef(FacelessJoePlugin.DEV_PREFIX + "_TESLA_TOWER_BODY_DEFAULT_SKIN_NAME",
                Assets.LoadAsset<Sprite>("texTeslaSkinDefault"),
                defaultRenderers,
                model);

            defaultSkin.meshReplacements = Modules.Skins.getMeshReplacements(defaultRenderers,
                "Base001",
                "Base Pillar 1",
                "Base Pillar 2",
                "Base Pillar 3",
                "Base Pillars001",

                "Base Tubes001",
                "Center Cylinder Wires001",
                "Center Cylinder001",
                "Center Orb001",

                "tower pole001",
                "tower pole001",
                "tower circle 006",
                "tower circle 007",
                "tower circle 008",
                "tower orb001");

            skins.Add(defaultSkin);
            #endregion

            #region mince

            SkinDef MCSkin = Modules.Skins.CreateSkinDef(FacelessJoePlugin.DEV_PREFIX + "_TESLA_TOWER_BODY_MC_SKIN_NAME",
                Assets.LoadAsset<Sprite>("texTeslaSkinMC"),
                defaultRenderers,
                model);

            MCSkin.meshReplacements = Modules.Skins.getMeshReplacements(defaultRenderers,
                "MCBase001",
                "MCBase Pillar 1",
                "MCBase Pillar 2",
                "MCBase Pillar 3",
                "MCBase Pillars001",

                "MCBase Tubes001",
                "MCCenter Cylinder Wires001",
                "MCCenter Cylinder001",
                "MCCenter Orb001",

                "MCtower pole001",
                "MCtower pole001",
                "MCtower circle 006",
                "MCtower circle 007",
                "MCtower circle 008",
                "MCtower orb001");
            
            MCSkin.rendererInfos[0].defaultMaterial = Modules.Materials.CreateHotpooMaterial("matTowerCobblestone");
            MCSkin.rendererInfos[1].defaultMaterial = Modules.Materials.CreateHotpooMaterial("matTowerRedstone");
            MCSkin.rendererInfos[2].defaultMaterial = Modules.Materials.CreateHotpooMaterial("matTowerRedstone");
            MCSkin.rendererInfos[3].defaultMaterial = Modules.Materials.CreateHotpooMaterial("matTowerRedstone");
            MCSkin.rendererInfos[4].defaultMaterial = Modules.Materials.CreateHotpooMaterial("matTowerRedstone");

            MCSkin.rendererInfos[5].defaultMaterial = Modules.Materials.CreateHotpooMaterial("matTowerIron");
            MCSkin.rendererInfos[6].defaultMaterial = Modules.Materials.CreateHotpooMaterial("matTowerCobblestone");
            MCSkin.rendererInfos[7].defaultMaterial = Modules.Materials.CreateHotpooMaterial("matTowerIron");
            MCSkin.rendererInfos[8].defaultMaterial = Modules.Materials.CreateHotpooMaterial("matTowerIron");

            MCSkin.rendererInfos[9].defaultMaterial = Modules.Materials.CreateHotpooMaterial("matTowerCobblestone");
            MCSkin.rendererInfos[10].defaultMaterial = Modules.Materials.CreateHotpooMaterial("WHITE");
            MCSkin.rendererInfos[11].defaultMaterial = Modules.Materials.CreateHotpooMaterial("matTowerQuartz");
            MCSkin.rendererInfos[12].defaultMaterial = Modules.Materials.CreateHotpooMaterial("matTowerQuartz");
            MCSkin.rendererInfos[13].defaultMaterial = Modules.Materials.CreateHotpooMaterial("matTowerQuartz");
            MCSkin.rendererInfos[14].defaultMaterial = Modules.Materials.CreateHotpooMaterial("matTowerDiamond");
            
            skins.Add(MCSkin);
            MCMinionSkinReplacement = new SkinDef.MinionSkinReplacement {
                minionBodyPrefab = bodyPrefab,
                minionSkin = MCSkin,
            };

            #endregion

            skinController.skins = skins.ToArray();
        }
    }
}