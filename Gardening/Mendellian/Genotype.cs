using UnityEngine;
using Exception = System.Exception;
using Random = UnityEngine.Random;
using GrandmaGreen.Collections;
using System.ComponentModel;

namespace GrandmaGreen.Garden
{
    [System.Serializable]
    public struct Genotype
    {
        public enum Size {
            [Description("XL")]
            VeryBig,
            [Description("L")]
            Big,
            [Description("M")]
            Medium,
            [Description("S")]
            Small,
            [Description("XS")]
            VerySmall
        };
        public enum Trait {
            [Description("Homozygous Dominant")]
            Dominant,
            [Description("Heterozygous")]
            Heterozygous,
            [Description("Homozygous Recessive")]
            Recessive
        };
        public enum Generation { P1, F1, F2 };

        public Size size;
        public Trait trait;
        public Generation generation;

        private bool a1;
        private bool a2;
        private bool b1;
        private bool b2;

        private bool p1;
        private bool f1;
        private bool f2;

        public Genotype(string genotype, Generation gen=Generation.P1)
        {
            // Validate & parse genotype str
            if (genotype.Length != 4)
            {
                throw new Exception("Invalid genotype: " + genotype); 
	        }
            string valid = "AaBb";
            foreach (char c in genotype)
            {
                if (!valid.Contains(c)) throw new Exception("Invalid genotype: " + genotype); 
	        }
            
            // Assign traits
            a1 = genotype[0] == 'A';
            a2 = genotype[1] == 'A';
            b1 = genotype[2] == 'B';
            b2 = genotype[3] == 'B';
            
            size = a1 && a2 ? Size.Big : (a1 || a2 ? Size.Medium : Size.Small);
            trait = b1 && b2 ? Trait.Dominant : (b1 || b2 ? Trait.Heterozygous : Trait.Recessive);
            
            // Track megas
            generation = gen;
            p1 = gen == Generation.P1;
            f1 = gen == Generation.F1;
            f2 = gen == Generation.F2;
        }

        public string SpriteSuffix(PlantId id)
        {
            string trait_suf = "";
            switch (trait)
            {
                case Trait.Recessive:
                    trait_suf = "Rec";
                    break;
                case Trait.Heterozygous:
                    trait_suf = "Het";
                    break;
                case Trait.Dominant:
                    trait_suf = "Dom";
                    break;
            }
            if (CollectionsSO.IsFlower(id) || CollectionsSO.IsVegetable(id))
            {
                string mega_suf = generation == Generation.F2 ? "_M" : "";
                return string.Format("_{0}{1}", trait_suf, mega_suf);
            }
            else if (CollectionsSO.IsFruit(id))
            {
                string size_suf = "";
                switch (size)
                {
                    case Size.Small:
                        size_suf = "S";
                        break;
                    case Size.Medium:
                        size_suf = "M";
                        break;
                    case Size.Big:
                        size_suf = "L";
                        break;
                    case Size.VeryBig:
                        size_suf = "L";
                        break;
                }
                return string.Format("_{0}_{1}", trait_suf, size_suf);
            }
            return "";
        }

        public float SpriteSize()
        {
            return size switch
            {
                Size.Small => 0.85f,
                Size.Medium => 1.0f,
                Size.Big => 1.15f,
                _ => 1.0f,
            };
        }

        public override bool Equals(object obj) =>
            obj is Genotype other
            && other.size == size
            && other.trait == trait
            && other.generation == generation;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return (a1 ? "A" : "a")
		        + (a2 ? "A" : "a")
		        + (b1 ? "B" : "b")
		        + (b2 ? "B" : "b");
        }

        public Genotype Cross(Genotype dad)
        {
            // build child genotype from punnett lookup
            int[][] punnettSquare = new int[][] {
                new int[] { 0, 4, 2, 6 }, new int[] { 0, 4, 3, 6 }, new int[] { 1, 4, 2, 6 }, new int[] { 1, 4, 3, 6 },
                new int[] { 0, 4, 2, 7 }, new int[] { 0, 4, 3, 7 }, new int[] { 1, 4, 2, 7 }, new int[] { 1, 4, 3, 7 },
                new int[] { 0, 5, 2, 6 }, new int[] { 0, 5, 3, 6 }, new int[] { 1, 5, 2, 6 }, new int[] { 1, 5, 3, 6 },
                new int[] { 0, 5, 2, 7 }, new int[] { 0, 5, 3, 7 }, new int[] { 1, 5, 2, 7 }, new int[] { 1, 5, 3, 7 }
            };
            Genotype mom = this;
            string cross = mom.ToString() + dad.ToString();
            string childGenotype = "";
            foreach (int i in punnettSquare[(int)(Random.value * punnettSquare.Length)])
            {
                childGenotype += cross[i];
            }

            Generation childGeneration = Generation.P1;
            // if parents are both F2, child is F2.
            if (mom.f2 && dad.f2)
            {
                childGeneration = Generation.F2;
            }
            // if parents are duplicate homozygous, child is F1.
            else if (mom.trait == dad.trait && mom.trait != Trait.Heterozygous)
            {
                childGeneration = Generation.F1;
                // if parents are duplicate homozygous and both F1, child is F2.
                if (mom.f1 && dad.f1)
                {
                    childGeneration = Generation.F2;
                }
                // if parents are duplicate homozygous and one is F1 and one is F2, child is F2.
                if (mom.f1 && dad.f2 || mom.f2 && dad.f1)
                {
                    childGeneration = Generation.F2;
                }
            }

            // --- debug
            string debug = "CrossBreed Debug (Click to Expand)\n";
            debug += "Mom: " + this.ToString() + " " + mom.generation.ToString() 
                +  " Dad: " + dad.ToString() + " " + dad.generation.ToString() + "\n";
            debug += "Punnett Square: \n";
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    foreach (int k in punnettSquare[i*4+j])
                    {
                        debug += cross[k];
		            }
                    debug += " ";
		        }
                debug += "\n";
	        }
            debug += "Child: " + childGenotype + " " + childGeneration.ToString();
            Debug.Log(debug);
            // ---

            return new Genotype(childGenotype, childGeneration);
        }
    }
}
