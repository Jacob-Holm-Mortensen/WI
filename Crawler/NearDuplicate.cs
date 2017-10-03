using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine
{
    class NearDuplicate
    {
        public string WithHash(string A, string B, int s)
        {
            List<string> AShinkles = GetShinkles(A.Replace(",", ""), s);
            List<string> BShinkles = GetShinkles(B.Replace(",", ""), s);
            double matches = 0, total = 0;
            // Convert shinkles into hash values

            // Find lowest value in each set

            // See if they match

            // Repeat with multible hash functions
            double similarity = (matches / total) * 100;
            if (similarity > 90)
                return "Having " + similarity.ToString() + "% " + s + "-shinkles in common, the two strings are near-duplicates";
            else
                return "Having " + similarity.ToString() + "% " + s + "-shinkles in common, the two strings are not near-duplicates";
        }

        public bool NoHash(string A, string B, int s)
        {
            List<string> AShinkles = GetShinkles(A.Replace(",", ""), s);
            List<string> BShinkles = GetShinkles(B.Replace(",", ""), s);
            double overlap = AShinkles.Intersect(BShinkles).Count();
            double union = AShinkles.Union(BShinkles).Count();
            double similarity = (overlap / union) * 100;
            if (similarity > 90)
                return true;
            else
                return false;
            /*if (similarity > 90)
                return "Having " + similarity.ToString() + "% " + s + "-shinkles in common, the two strings are near-duplicates";
            else
                return "Having " + similarity.ToString() + "% " + s + "-shinkles in common, the two strings are not near-duplicates";*/
        }

        public List<string> GetShinkles(string sentence, int shinkleSize)
        {
            List<string> Shinkles = new List<string>();
            List<string> Split = new List<string>();
            Split.AddRange(sentence.Split());
            for (int i = 0; i < Split.Count() - shinkleSize + 1; i++)
            {
                string shinkle = Split[i];
                for (int j = 1; j < shinkleSize; j++)
                {
                    shinkle = shinkle + " " + Split[i + j];
                }
                Shinkles.Add(shinkle);
            }
            if (Split.Count() < shinkleSize)
            {
                Shinkles = Split;
            }
            return Shinkles;
        }
    }
}
