﻿using Vampire.Defs;
using Vampire.Hediffs;
using Verse;

namespace Vampire.Disciplines.Thaumaturgy
{
    public class DisciplineEffect_BloodShield : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            HealthUtility.AdjustSeverity(target, VampDefOf.ROMV_BloodShieldHediff, 1.0f);
            if (target.health.hediffSet.GetFirstHediffOfDef(VampDefOf.ROMV_BloodShieldHediff) is Hediff hd &&
                hd.TryGetComp<HediffComp_Shield>() is HediffComp_Shield shield)
            {
                shield.NotifyRefilled();
            }
        }
    }
}
