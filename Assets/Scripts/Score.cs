using System;

public class Score : IComparable<Score> {
    public string playerName;
    public float score;

    public Score(string playerName, float score) {
        this.playerName = playerName;
        this.score = score;
    }

    public string Serialize() {
        return playerName + "-" + score;
    }

    public override string ToString() {
        return Serialize();
    }

    public override bool Equals(object obj) {
        return obj != null && obj is Score && score == ((Score) obj).score &&
               playerName == ((Score) obj).playerName;
    }

    public override int GetHashCode() {
        return Serialize().GetHashCode();
    }

    public int CompareTo(Score other) {
        if (score == other.score)
            return 0;
        if (score < other.score)
            return -1;
        return 1;
    }

    public static Score Deserialize(string serialization) {
        string[] parts = serialization.Split('-');
        string playerName = parts[0];
        float score = float.Parse(parts[1]);
        return new Score(playerName, score);
    }
}
