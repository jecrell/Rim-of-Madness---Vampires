﻿using RimWorld;
using Vampire.Defs;
using Verse;

namespace Vampire.Disciplines.Dominate
{
    public class DisciplineEffect_Sleep : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            HealthUtility.AdjustSeverity(target, VampDefOf.ROMV_SleepHediff, 1.0f);
            MoteMaker.ThrowMetaIcon(target.Position, target.Map, ThingDefOf.Mote_SleepZ);
        }
    }
}
