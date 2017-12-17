﻿using Verse;

namespace Vampire.Hediffs
{
    public class HediffComp_Possession : HediffComp_Disappears
    {

        public new HediffCompProperties_Possession Props
        {
            get
            {
                return (HediffCompProperties_Possession)props;
            }
        }
        
        public void ActivateEffect(Pawn activator)
        {
            string text = Pawn.LabelIndefinite();
            if (Pawn.guest != null)
            {
                Pawn.guest.SetGuestStatus(null);
            }
            bool flag = Pawn.Name != null;
            if (Pawn.Faction != activator.Faction)
            {
                Pawn.SetFaction(activator.Faction, Pawn);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (CompShouldRemove)
            {
                HealthUtility.AdjustSeverity(Pawn, HediffDef.Named("HeartAttack"), 1.0f);
            }
        }
    }
}
