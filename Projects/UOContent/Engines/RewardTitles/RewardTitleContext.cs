using System.Collections.Generic;
using ModernUO.Serialization;
using Server.Mobiles;

namespace Server.Engines.RewardTitles;

[PropertyObject]
[SerializationGenerator(0)]
public partial class RewardTitleContext
{
    [SerializableField(0, setter: "private")]
    private List<TextDefinition> _titles;

    // -1 = no title selected (hidden); written unconditionally so the default is explicit.
    [SerializableField(1, setter: "private")]
    private int _selected;

    private readonly PlayerMobile _player;

    public PlayerMobile Player => _player;

    public RewardTitleContext(PlayerMobile player)
    {
        _player = player;
        _titles = new List<TextDefinition>();
        _selected = -1;
    }

    public IReadOnlyList<TextDefinition> TitleList => _titles;

    public TextDefinition SelectedTitle =>
        _selected >= 0 && _selected < _titles.Count ? _titles[_selected] : null;

    public override string ToString() => "Reward Titles";

    public bool Add(TextDefinition title)
    {
        if (title == null || title.IsEmpty || _titles.Contains(title))
        {
            return false;
        }

        _titles.Add(title);
        return true;
    }

    public void Select(int index)
    {
        if (index >= -1 && index < _titles.Count)
        {
            _selected = index;
        }
    }

    public bool Remove(TextDefinition title)
    {
        var idx = _titles.IndexOf(title);
        if (idx < 0)
        {
            return false;
        }

        _titles.RemoveAt(idx);

        if (_selected == idx)
        {
            _selected = -1;
        }
        else if (_selected > idx)
        {
            _selected--;
        }

        return true;
    }
}
