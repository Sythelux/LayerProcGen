using System;
using System.Collections.Generic;
using System.Linq;

class StochasticLSystem
{
    readonly string axiom;
    readonly Dictionary<char,Func<string>> rules;
    readonly System.Random rng;

    public StochasticLSystem(string ax, Dictionary<char,Func<string>> r,
                             System.Random rando)
        => (axiom,rules,rng) = (ax,r,rando);

    public string Generate(int iterations)
    {
        string s = axiom;
        for (int i=0;i<iterations;i++)
            s = string.Concat(s.Select(c =>
                 rules.TryGetValue(c,out var f) ? f() : c.ToString()));
        return s;
    }
}
