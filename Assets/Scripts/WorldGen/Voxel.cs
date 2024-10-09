

public struct Voxel
{
    public byte voxID;
    public bool isSolid
    {
        get
        {
            return (voxID != 0);
        }
    }
}
