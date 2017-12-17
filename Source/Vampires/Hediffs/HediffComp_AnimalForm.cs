﻿using System.Linq;
using System.Text;
using Vampire.Utilities;
using Verse;

namespace Vampire.Hediffs
{
    public class HediffComp_AnimalForm : HediffComp_Disappears
    {
        private bool activated = false;
        public bool Activated => activated;

        private Graphic bodyGraphic = null;
        public Graphic BodyGraphic {
            get => bodyGraphic;
            set => bodyGraphic = value; }

        public new HediffCompProperties_AnimalForm Props
        {
            get
            {
                return (HediffCompProperties_AnimalForm)props;
            }
        }

        public override string CompTipStringExtra
        {
            get
            {
                StringBuilder s = new StringBuilder();
                s.AppendLine("ROMV_HI_BodySize".Translate(Props.animalToChangeInto.RaceProps.baseBodySize.ToStringPercent()));
                s.AppendLine("ROMV_HI_HealthScale".Translate(Props.animalToChangeInto.RaceProps.baseHealthScale.ToStringPercent()));
                return s.ToString();
            }
        }
        
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (!activated)
            {
                activated = true;
                if (Pawn.health.hediffSet.hediffs.FirstOrDefault(x => x != parent && x.TryGetComp<HediffComp_AnimalForm>() != null) is HediffWithComps h)
                {
                    Pawn.health.hediffSet.hediffs.Remove(h);
                }
                Pawn.VampComp().CurrentForm = Props.animalToChangeInto;
                Pawn.VampComp().CurFormGraphic = null;

                //Log.Message("CurrentForm set to " + this.Props.animalToChangeInto.label);
            }
            if (CompShouldRemove)
            {
                Pawn.VampComp().CurrentForm = null;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<bool>(ref activated, "activated");
        }
    }
}
