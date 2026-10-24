using System;
using System.Collections.Generic;

namespace NepalTrial.Trial
{
    /// <summary>
    /// Scoring rule definitions for connected A/K and B training exams.
    /// The source is Bagmati's 2077 directive (Annex 5/6); sensors must separately
    /// distinguish a line TOUCH from a tyre crossing before using the touch rules.
    /// No claim is made that all current sensors can detect these events yet.
    /// </summary>
    public static class NepalTrialScoringCatalog
    {
        public readonly struct Rule
        {
            public readonly string Code;
            public readonly int Points;
            public readonly int FailOnOccurrence;
            public Rule(string code,int points,int failOnOccurrence=0)
            {
                Code=code;
                Points=points;
                FailOnOccurrence=failOnOccurrence;
            }
        }

        public static int MaximumMarks(string stage,bool twoWheeler)
        {
            switch (stage)
            {
                case "figure-eight": return 20;
                case "narrow-plank": return twoWheeler ? 10 : 0;
                case "u-turn": return 10;
                case "traffic-light": return 15;
                case "speed-breaker": return twoWheeler ? 15 : 10;
                case "hill-start": return twoWheeler ? 0 : 15;
                case "ramp": return twoWheeler ? 20 : 0;
                case "garage": return twoWheeler ? 0 : 20;
                case "behavior": return 10;
                default: return 0;
            }
        }

        public static IList<Rule> GetRules(bool twoWheeler)
        {
            var rules = new List<Rule>
            {
                // The existing sensors report boundary *departures*, not a gentle
                // painted-line touch. Leaving the lane is immediately disqualifying.
                new Rule("figure-eight-boundary",0,1),
                new Rule("figure-eight-reverse",0,1),
                new Rule("figure-eight-line-touch",5,twoWheeler?2:3),
                new Rule("u-turn-boundary",0,1),
                new Rule("u-turn-incomplete",0,1),
                new Rule("u-turn-timeout",0,1),
                new Rule("u-turn-no-indicator",10),
                new Rule("signal-red",15),
                new Rule("signal-yellow",15),
                new Rule("behavior-check",3),
                new Rule("behavior-safety",4),
                new Rule("behavior-control",3),
                new Rule("obstacle-hit",0,1)
            };
            if (twoWheeler)
            {
                rules.Add(new Rule("foot-down",5,2));
                rules.Add(new Rule("two-wheeler-fall",0,1));
                rules.Add(new Rule("plank-off",0,1));
                rules.Add(new Rule("plank-foot-down",0,1));
                rules.Add(new Rule("plank-timeout",0,1));
                rules.Add(new Rule("breaker-foot-down",5,2));
                rules.Add(new Rule("breaker-speed",0)); // not a defined points deduction
                rules.Add(new Rule("breaker-first-stall",5));
                rules.Add(new Rule("breaker-repeat-stall",0,1));
                rules.Add(new Rule("figure-eight-stall",0,1));
                rules.Add(new Rule("ramp-rollback",0,1));
                rules.Add(new Rule("ramp-no-stop",0,1));
                rules.Add(new Rule("ramp-foot-down",0,1));
                rules.Add(new Rule("ramp-timeout",0,1));
                rules.Add(new Rule("ramp-first-stall",0));
                rules.Add(new Rule("ramp-repeat-stall",0,1));
                rules.Add(new Rule("engine-stall",5));
            }
            else
            {
                rules.Add(new Rule("hill-rollback",0,1));
                rules.Add(new Rule("hill-no-stop",0,1));
                rules.Add(new Rule("figure-eight-stall",0,1));
                rules.Add(new Rule("hill-timeout",0,1));
                rules.Add(new Rule("hill-first-stall",0));
                rules.Add(new Rule("hill-repeat-stall",0,1));
                rules.Add(new Rule("garage-obstruction",0,1));
                rules.Add(new Rule("garage-forward-correction",10,2));
                rules.Add(new Rule("garage-line-touch",0,1));
                rules.Add(new Rule("breaker-speed",0));
                rules.Add(new Rule("breaker-first-stall",5));
                rules.Add(new Rule("breaker-repeat-stall",0,1));
            }
            return rules;
        }
    }
}
