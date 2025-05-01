using System.Collections.Generic;
using System.Text;

/*
 * L-System implementation for procedural generation.
 * This class generates a string based on a set of rules and a seed string.
 * The generated string can be used to create complex structures like trees, plants, etc.
 *
 * Example usage:
 * 
 * Suppose alphabet is:
 * A, B, C, D, [ and ].
 * 
 * Then define the rules as:
 *
 * var rules = new Dictionary<char, string> {
 *   {'A', "ADA"},
 *   {'B', "D[B]D[B]"},
 *   {'C', "ACA"},
 *   {'D', "DA"}
 * };
 *
 * The seed string is "B". Construct the LSystem object:
 *
 * var lSystem = new LSystem("B", rules);
 * string result = lSystem.Generate(4);
 *
 * Then the result will be "D[B]D[B]A[D[A[D[A]]]]D[A[D[A[D[A]]]]"
 */
public class LSystem {
    private Dictionary<char, string> rules;
    private string currentString;

    public LSystem(string seed, Dictionary<char, string> rules) {
        this.currentString = seed;
        this.rules = rules;
    }

    public string Generate(int iterations) {
        StringBuilder sb = new StringBuilder(currentString);

        for (int i = 0; i < iterations; i++) {
            StringBuilder next = new StringBuilder();

            foreach (char c in sb.ToString()) {
                if (rules.ContainsKey(c))
                    next.Append(rules[c]);
                else
                    next.Append(c);
            }

            sb = next;
        }

        currentString = sb.ToString();
        return currentString;
    }
}
