﻿using Verse;

namespace Vampire.Hediffs
{
    public class HediffCompProperties_AnimalForm : HediffCompProperties_Disappears
    {
        public PawnKindDef animalToChangeInto;
        public bool immuneTodamage = false;
        public bool canGiveDamage = true;
        public HediffCompProperties_AnimalForm()
        {
            compClass = typeof(HediffComp_AnimalForm);
        }
    }
}
