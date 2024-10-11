

public struct Voxel
{
    public int ID;
    public bool isSolid
    {
        get
        {
            return (ID != 0);
        }
    }
}
