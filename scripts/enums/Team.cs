using Godot;
using System;

public enum Team {
    None,
    Fire,
    Earth,
}

public struct TeamInfo {
    public static Color GetTeamColor(Team team) {
        switch (team) {
            case Team.Fire:
                return new Color(0.745f, 0.302f, 0.145f);
            case Team.Earth:
                return new Color(0.427f, 1, 0);
            default:
                return Color.FromHsv(0, 0, 0.75f);
        }
    }
}