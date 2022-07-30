using EntityStates;
using RoR2;

namespace ModdedEntityStates.TeslaTrooper.Tower {
    public class TowerLifetime : BaseSkillState {
        public static float skillsPlusSeconds = 0;

        public override void OnEnter() {
            base.OnEnter();
            Helpers.LogWarning(skillsPlusSeconds);
        }

        public override void FixedUpdate() {
            base.FixedUpdate();
        }
    }

}
