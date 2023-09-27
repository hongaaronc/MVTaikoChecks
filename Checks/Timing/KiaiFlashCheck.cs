﻿using System.Collections.Generic;

using MapsetParser.objects;
using MapsetParser.objects.timinglines;
using MapsetParser.statics;

using MapsetVerifierFramework.objects;
using MapsetVerifierFramework.objects.attributes;
using MapsetVerifierFramework.objects.metadata;

using MVTaikoChecks.Utils;

using static MVTaikoChecks.Aliases.Mode;
using static MVTaikoChecks.Aliases.Level;

namespace MVTaikoChecks.Checks.Timing
{
    [Check]
    public class KiaiFlashCheck : BeatmapCheck
    {
        private const string _MINOR = nameof(_MINOR);
        private const string _WARNING = nameof(_WARNING);

        public override CheckMetadata GetMetadata() =>
            new BeatmapCheckMetadata()
            {
                Author = "Hivie",
                Category = "Timing",
                Message = "Kiai flashes",
                Modes = new Beatmap.Mode[] { MODE_TAIKO },
                Documentation = new Dictionary<string, string>()
                {
                    {
                        "Purpose",
                        @"
                    Ensuring that there are no fast kiai flashes."
                    },
                    {
                        "Reasoning",
                        @"
                    Kiai flashes in osu!taiko cause the entire playfield to flash, which can cause performance problems, alongside potentially causing epileptic effects if abused."
                    }
                }
            };

        public override Dictionary<string, IssueTemplate> GetTemplates() =>
            new Dictionary<string, IssueTemplate>()
            {
                {
                    _MINOR,
                    new IssueTemplate(LEVEL_MINOR, "{0} Kiai flash ", "timestamp - ").WithCause(
                        "A kiai flash exists, but is not too drastic"
                    )
                },
                {
                    _WARNING,
                    new IssueTemplate(LEVEL_WARNING, "{0} Kiai flash", "timestamp - ").WithCause(
                        "A kiai flash that's too drastic exists"
                    )
                }
            };

        public override IEnumerable<Issue> GetIssues(Beatmap beatmap)
        {
            foreach (var line in beatmap.timingLines)
            {
                if (line.kiai != line.Next(skipConcurrent: true).kiai)
                {
                    var timing = beatmap.GetTimingLine<UninheritedLine>(line.offset);
                    var normalizedMsPerBeat = timing.GetNormalizedMsPerBeat();
                    double gap = line.Next(skipConcurrent: true).offset - line.offset;

                    if (gap < normalizedMsPerBeat / 2.5)
                    {
                        yield return new Issue(
                            GetTemplate(_WARNING),
                            beatmap,
                            Timestamp.Get(line.offset)
                        );
                    }
                    else if (gap < normalizedMsPerBeat / 2)
                    {
                        yield return new Issue(
                            GetTemplate(_MINOR),
                            beatmap,
                            Timestamp.Get(line.offset)
                        );
                    }
                }
            }
        }
    }
}