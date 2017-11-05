﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trady.Analysis.Infrastructure;
using Trady.Core;
using Trady.Core.Infrastructure;

namespace Trady.Analysis.Indicator
{
    public class PlusDirectionalIndicator<TInput, TOutput> : NumericAnalyzableBase<TInput, (decimal High, decimal Low, decimal Close), TOutput>
    {
        private PlusDirectionalMovementByTuple _pdm;
        private MinusDirectionalMovementByTuple _mdm;
        private readonly GenericMovingAverage _tpdmEma;
        private readonly AverageTrueRangeByTuple _atr;

        public PlusDirectionalIndicator(IEnumerable<TInput> inputs, Func<TInput, (decimal High, decimal Low, decimal Close)> inputMapper, int periodCount) : base(inputs, inputMapper)
        {
            _pdm = new PlusDirectionalMovementByTuple(inputs.Select(i => inputMapper(i).High));
            _mdm = new MinusDirectionalMovementByTuple(inputs.Select(i => inputMapper(i).Low));

            Func<int, decimal?> tpdm = i => (_pdm[i] > 0 && _pdm[i] > _mdm[i]) ? _pdm[i] : 0;

            _tpdmEma = new GenericMovingAverage(
                periodCount,
                i => Enumerable.Range(i - periodCount + 1, periodCount).Average(tpdm),
                tpdm,
                Smoothing.Mma(periodCount),
                inputs.Count());

            _atr = new AverageTrueRangeByTuple(inputs.Select(inputMapper), periodCount);

            PeriodCount = periodCount;
        }

        public int PeriodCount { get; }

        protected override decimal? ComputeByIndexImpl(IReadOnlyList<(decimal High, decimal Low, decimal Close)> mappedInputs, int index)
            => _tpdmEma[index] / _atr[index] * 100;
    }

    public class PlusDirectionalIndicatorByTuple : PlusDirectionalIndicator<(decimal High, decimal Low, decimal Close), decimal?>
    {
        public PlusDirectionalIndicatorByTuple(IEnumerable<(decimal High, decimal Low, decimal Close)> inputs, int periodCount)
            : base(inputs, i => i, periodCount)
        {
        }
    }

    public class PlusDirectionalIndicator : PlusDirectionalIndicator<IOhlcvData, AnalyzableTick<decimal?>>
    {
        public PlusDirectionalIndicator(IEnumerable<IOhlcvData> inputs, int periodCount)
            : base(inputs, i => (i.High, i.Low, i.Close), periodCount)
        {
        }
    }
}