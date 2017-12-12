﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using AbilityUser;

namespace Vampire
{
    public class VampireCorpse : Corpse
    {

        public VampireCorpse() : base()
        {
            this.operationsBillStack = new BillStack(this);
            this.innerContainer = new ThingOwner<Pawn>(this, true, LookMode.Reference);
        }

        private bool ShouldVanish
        {
            get
            {
                return this.InnerPawn.RaceProps.Animal && this.vanishAfterTimestamp > 0 && this.Age >= this.vanishAfterTimestamp && base.Spawned && this.GetRoom(RegionType.Set_Passable) != null && this.GetRoom(RegionType.Set_Passable).TouchesMapEdge && !base.Map.roofGrid.Roofed(base.Position);
            }
        }

        private BodyPartRecord GetBestBodyPartToEat(Pawn ingester, float nutritionWanted)
        {
            IEnumerable<BodyPartRecord> source = from x in this.InnerPawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
                                                 where x.depth == BodyPartDepth.Outside && FoodUtility.GetBodyPartNutrition(this.InnerPawn, x) > 0.001f
                                                 select x;
            if (!source.Any<BodyPartRecord>())
            {
                return null;
            }
            return source.MinBy((BodyPartRecord x) => Mathf.Abs(FoodUtility.GetBodyPartNutrition(this.InnerPawn, x) - nutritionWanted));
        }

        private void NotifyColonistBar()
        {
            if (this.InnerPawn.Faction == Faction.OfPlayer && Current.ProgramState == ProgramState.Playing)
            {
                Find.ColonistBar.MarkColonistsDirty();
            }
        }

        private ThingOwner<Pawn> innerContainer;

        private int timeOfDeath = -1000;

        private int vanishAfterTimestamp = -1000;

        private BillStack operationsBillStack;
        
        private const int VanishAfterTicksSinceDessicated = 6000000;

        
        ///VAMPIRE CORPSE ITEMS ////////////////////////////////////////////////////////////////////////////////////////////////////
        
        private int bloodPoints = -1;
        public int BloodPoints { get => bloodPoints; set => bloodPoints = value; }

        private bool burnedToAshes = false;
        public bool BurnedToAshes { get => burnedToAshes;
            set
            {
                if (value == true)
                {
                    if (InnerPawn is Pawn p)
                    {
                        List<BodyPartRecord> parts = p.health.hediffSet.GetNotMissingParts().ToList();
                        foreach (BodyPartRecord rec in parts)
                        {
                            if (p.health.hediffSet.PartIsMissing(rec))
                            {
                                continue;
                            }
                            HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(DamageDefOf.Burn, p, rec);
                            Hediff_Injury hediff_Injury = (Hediff_Injury)HediffMaker.MakeHediff(hediffDefFromDamage, p, null);
                            hediff_Injury.Part = rec;
                            hediff_Injury.source = null;
                            hediff_Injury.sourceBodyPartGroup = null;
                            hediff_Injury.sourceHediffDef = null;
                            hediff_Injury.Severity = 999999;
                            p.health.AddHediff(hediff_Injury, null, new DamageInfo?(new DamageInfo(DamageDefOf.Burn, 999999, -1, null, rec)));
                        }
                    }
                }
                burnedToAshes = value;
            }
        }

        private bool diableried = false;
        public bool Diableried { get => diableried; set => diableried = value; }
        

        private Graphic ashesCache = null;
        public Graphic Ashes
        {
            get
            {
                if (ashesCache == null)
                {
                    GraphicData temp = new GraphicData();
                    temp.texPath = "Things/Item/Resource/VampireAshes";
                    temp.graphicClass = typeof(Graphic_Single);
                    ashesCache =  temp.Graphic;
                }
                return ashesCache;
            }
        }

        public override string Label
        {
            get
            {
                if (Diableried)
                    return "ROMV_SoullessHuskOf".Translate(base.Label);
                return (burnedToAshes) ? "ROMV_AshesOf".Translate(base.Label) : base.Label;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder s = new StringBuilder();

            if (!burnedToAshes)
            {
                if (base.GetInspectString() is string baseString && baseString != "")
                    s.AppendLine(baseString);
                s.AppendLine("Blood points remaining: " + bloodPoints);
            }
            else
            {
                if (this.InnerPawn.Faction != null)
                {
                    s.AppendLine("Faction".Translate() + ": " + this.InnerPawn.Faction.Name);
                }
                s.AppendLine("DeadTime".Translate(new object[]
                {
                this.Age.ToStringTicksToPeriod(false, false, true)
                }));
            }
            return s.ToString().TrimEndNewlines();
        }
        

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Building building = this.StoringBuilding();
            if (building != null && building.def == ThingDefOf.Grave)
            {
                return;
            }
            if (!burnedToAshes)
                this.InnerPawn.Drawer.renderer.RenderPawnAt(drawLoc);
            else
                Ashes.Draw(drawLoc, Rot4.North, this, 0);

        }

        public bool CanResurrect => this.InnerPawn != null && !BurnedToAshes && this.InnerPawn.Faction == Faction.OfPlayerSilentFail && !Diableried && this.GetRotStage() < RotStage.Dessicated;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
                yield return g;

            Vampire.VitaeAbilityDef bloodResurrection = DefDatabase<Vampire.VitaeAbilityDef>.GetNamedSilentFail("ROMV_VampiricResurrection");
            if (CanResurrect)
            {
                yield return new Command_Action()
                {
                    defaultLabel = bloodResurrection.label,
                    defaultDesc = bloodResurrection.GetDescription(),
                    icon = bloodResurrection.uiIcon,
                    action = delegate
                    {
                        Pawn AbilityUser = this.InnerPawn;
                        AbilityUser.Drawer.Notify_DebugAffected();
                        ResurrectionUtility.Resurrect(AbilityUser);
                        MoteMaker.ThrowText(AbilityUser.PositionHeld.ToVector3(), AbilityUser.MapHeld, StringsToTranslate.AU_CastSuccess, -1f);
                        AbilityUser.BloodNeed().AdjustBlood(-99999999);
                        HealthUtility.AdjustSeverity(AbilityUser, VampDefOf.ROMV_TheBeast, 1.0f);
                        MentalStateDef MentalState_VampireBeast = DefDatabase<MentalStateDef>.GetNamed("ROMV_VampireBeast");
                        AbilityUser.mindState.mentalStateHandler.TryStartMentalState(MentalState_VampireBeast, null, true, false, null);
                    },
                    disabled = false
                };
            }
        }

        // Token: 0x060047A3 RID: 18339 RVA: 0x002079C4 File Offset: 0x00205DC4
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<int>(ref this.bloodPoints, "bloodPoints", -1);
            Scribe_Values.Look<bool>(ref this.burnedToAshes, "burnedToAshes", false);
            Scribe_Values.Look<bool>(ref this.diableried, "diableried", false);

            Scribe_Values.Look<int>(ref this.timeOfDeath, "timeOfDeath", 0, false);
            Scribe_Values.Look<int>(ref this.vanishAfterTimestamp, "vanishAfterTimestamp", 0, false);
            Scribe_Values.Look<bool>(ref this.everBuriedInSarcophagus, "everBuriedInSarcophagus", false, false);
            Scribe_Deep.Look<BillStack>(ref this.operationsBillStack, "operationsBillStack", new object[]
            {
                this
            });
            Scribe_Deep.Look<ThingOwner<Pawn>>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
        }
    }
}