using NoxLibrary;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wind : IByteListAppendable
{
    public Degrees Angle { get; set; }
    public float Speed { get; set; }

    public Degrees OriginalAngle { get; set; }
    public float OriginalSpeed { get; set; }
    public float BigChangeCurrentChance { get; set; }

    public float RelativeSpeed => Speed / MaxSpeed;
    public Vector2 VelocityVector => new Vector2(Speed * (float)(Radians)Angle, Speed * (float)(Radians)Angle);

    #region Settings
    public static int SmallChangeMaxVariation { get; set; }
    public static int SmallSpeedMaxVariation { get; set; }
    public static int GustVariation { get; set; }
    public static int GustChance { get; set; }
    public static int BigChangeChance { get; set; }
    public static int IncreaseChancePerTurn { get; set; }
    public static int MaxSpeed { get; set; }
    #endregion

    #region Constructors

    public static Wind Zero => new Wind(0, 0);

    private Wind()
    {
        BigChangeCurrentChance = BigChangeChance;
        Change(forceRandomChange: true);
    }

    public Wind(short angle, short speed)
    {
        BigChangeCurrentChance = BigChangeChance;
        OriginalAngle = Angle = angle;
        OriginalSpeed = Speed = speed;
    }

    protected Wind(short angle, byte speed, byte bigChangeCurrentChance)
    {
        this.BigChangeCurrentChance = bigChangeCurrentChance;
        OriginalAngle = Angle = angle;
        OriginalSpeed = Speed = speed;
    }

    public Wind(Vector2 velocityVector)
    {
        BigChangeCurrentChance = BigChangeChance;
        OriginalSpeed = Speed = velocityVector.magnitude.RoundToInt();
        OriginalAngle = Angle = Math.Acos(velocityVector.normalized.x).RoundToInt();
    }

    #endregion

    public void Change(bool forceRandomChange = false)
    {
        if (forceRandomChange || Random.Range(0, 100) < BigChangeCurrentChance)
        {
            OriginalAngle = Angle = Random.Range(0, 360);
            OriginalSpeed = Speed = Random.Range(0, MaxSpeed + 1);

            BigChangeCurrentChance = BigChangeChance;
        }
        else
        {
            Angle = (OriginalAngle + Random.Range(-SmallChangeMaxVariation, SmallChangeMaxVariation + 1)).Validade();
            Speed = Random.Range(0, 100) < GustChance
                ? Math.Min(Speed + GustVariation, MaxSpeed)
                : Mathn.Clamp(OriginalSpeed + Random.Range(-SmallSpeedMaxVariation, SmallSpeedMaxVariation + 1), 0, MaxSpeed);

            BigChangeCurrentChance += IncreaseChancePerTurn;
        }
    }

    public static Wind CreateFromBytes(ByteArray ba)
        => new Wind(ba.GetShort(), ba.GetByte(), ba.GetByte());

    public void UpdateFromBytes(ByteArray ba)
    {
        Angle = ba.GetShort();
        Speed = ba.GetByte();
        BigChangeCurrentChance = ba.GetByte();
    }

    public ByteList AppendBytes(ByteList bl) => bl
        .Append((short)Angle)
        .Append((byte)Speed)
        .Append((byte)BigChangeCurrentChance);

    public bool SameVector(Wind other) => Angle == other.Angle && Speed == other.Speed;

    public bool WasLastChangeBig => BigChangeCurrentChance == BigChangeChance; //Only works if IncreaseChancePerTurn != 0
}