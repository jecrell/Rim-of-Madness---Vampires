﻿using Vampire.Defs;
using Verse;

namespace Vampire.Disciplines.Auspex
{
    public class DisciplineEffect_HeightenedSenses : Verb_UseAbilityPawnEffect
    {
        public override void Effect(Pawn target)
        {
            base.Effect(target);
            HealthUtility.AdjustSeverity(target, VampDefOf.ROMV_HeightenedSensesHediff, 1.0f);
        }
    }
}