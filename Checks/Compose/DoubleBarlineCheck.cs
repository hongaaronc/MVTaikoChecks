﻿using System.Collections.Generic;
using System.Linq;

using MapsetParser.objects;
using MapsetParser.objects.hitobjects;
using MapsetParser.objects.timinglines;
using MapsetParser.statics;

using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;

using MVTaikoChecks.Utils;

using static MVTaikoChecks.Global;
using static MVTaikoChecks.Aliases.Difficulty;
using static MVTaikoChecks.Aliases.Mode;
using static MVTaikoChecks.Aliases.Level;

namespace MVTaikoChecks.Checks.Compose
{
    [Check]
    public class DoubleBarlineCheck : BeatmapCheck
    {
        private const string _PROBLEM = nameof(_PROBLEM);
        private const string _WARNING = nameof(_WARNING);


        private readonly Beatmap.Difficulty[] _DIFFICULTIES = new Beatmap.Difficulty[] { DIFF_KANTAN, DIFF_FUTSUU, DIFF_MUZU, DIFF_ONI, DIFF_INNER, DIFF_URA };

        public override CheckMetadata GetMetadata() => new BeatmapCheckMetadata()
        {
            Author = "Hivie & Phob",
            Category = "Compose",
            Message = "Double barlines",

            Difficulties = _DIFFICULTIES,

            Modes = new Beatmap.Mode[]
            {
                MODE_TAIKO
            },

            Documentation = new Dictionary<string, string>()
            {
                {
                    "Purpose",
                    "Ensuring that there are no two barlines within 50ms of each other."
                },
                {
                    "Reasoning",
                    "Double barlines are caused by rounding errors, visually disruptive and confusing in the representation of a song's downbeat."
                }
            }
        };

        public override Dictionary<string, IssueTemplate> GetTemplates() => new Dictionary<string, IssueTemplate>()
        {
            {
                _PROBLEM,
                new IssueTemplate(LEVEL_PROBLEM,
                    "{0} Double barline",
                    "timestamp - ")
            },
            {
                _WARNING,
                new IssueTemplate(LEVEL_WARNING,
                    "{0} Potential double barline, doublecheck manually",
                    "timestamp - ")
            }
        };

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            const double threshold = 50;

            var redLines = beatmap.timingLines
                .Where(x => x is UninheritedLine)
                .Select(x => x as UninheritedLine)
                .ToList();

            for (int i = 0; i < redLines.Count; i++)
            {
                var current = redLines[i];
                var next = redLines.SafeGetIndex(i + 1);

                var barlineGap = current.msPerBeat * current.meter;
                var distance = (next?.offset ?? double.MaxValue) - current.offset;

                // if the next line has an omit, double barlines can't happen
                // if the current line has an omit and lasts only 1 measure, double barlines can't happen either
                // true for not insanely high bpms, but who cares ^
                if (next == null || next.omitsBarLine || (current.omitsBarLine && distance <= barlineGap))
                    continue;

                var rest = distance % barlineGap;

                if (rest - threshold <= 0 && rest > 0)
                {
                    if (rest >= 0.5)
                    {
                        yield return new Issue(
                            GetTemplate(_PROBLEM),
                            beatmap,
                            Timestamp.Get(next.offset)
                        ).ForDifficulties(_DIFFICULTIES);
                    }
                    else
                    {
                        yield return new Issue(
                            GetTemplate(_WARNING),
                            beatmap,
                            Timestamp.Get(next.offset)
                        ).ForDifficulties(_DIFFICULTIES);
                    }
                }
            }
        }
    }
}
