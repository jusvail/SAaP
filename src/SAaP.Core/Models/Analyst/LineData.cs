using System.Collections.Generic;

namespace SAaP.Core.Models.Analyst;

public class LineData
{
    /// <summary>
    /// form of X day line
    /// </summary>
    public static readonly Dictionary<LineForm, int> FormOfDayLine = new()
    {
        { LineForm.D5, 5 },
        { LineForm.D10, 10 },
        { LineForm.D20, 20 },
        { LineForm.D50, 50 },
        { LineForm.D120, 120 },
        { LineForm.D150, 150 },
        { LineForm.D200, 200 }
    };

    /// <summary>
    /// form of X rps line
    /// </summary>
    public static readonly Dictionary<LineForm, int> FormOfRpsLine = new()
    {
        { LineForm.Rps20, 20 },
        { LineForm.Rps50, 50 },
        { LineForm.Rps120, 120 },
        { LineForm.Rps250, 150 }
    };

    public LineData()
    {
        foreach (var lineForm in FormOfDayLine.Keys)
        {
            _lineForms.Add(lineForm, new List<double>());
        }
        foreach (var lineForm in FormOfRpsLine.Keys)
        {
            _lineForms.Add(lineForm, new List<double>());
        }
        _lineForms.Add(LineForm.Cci,new List<double>());
    }

    public static readonly double BlankValue = -1;

    public static readonly KeyValuePair<LineForm, int> FormOfCciLine = new(LineForm.Cci, 14);

    private readonly Dictionary<LineForm, List<double>> _lineForms = new();

    public List<double> this[LineForm form]
    {
        get => _lineForms[form];
        set => _lineForms[form] = value;
    }
}