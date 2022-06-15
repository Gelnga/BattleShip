using System;

namespace BattleShipBrain;

public class ShipRotationLogic
{
    public Func<int, bool>? XComparisonLogic;
    public Func<int, int>? XMovementLogic;
    public Func<int, bool>? YComparisonLogic;
    public Func<int, int>? YMovementLogic;
    
    // 0 - east, 1 - north, 2 - west, 3 - south
    public ShipRotationLogic(int rotationSide, Coordinate startingPoint, ShipConfig config)
    {
        var sxsize = config.ShipSizeX;
        var sysize = config.ShipSizeY;
        
        switch (rotationSide)
        {
            case 0:
                break;
            case 1:
                (sxsize, sysize) = (sysize, sxsize);
                XComparisonLogic = xCurrentValue =>
                    startingPoint.X - sxsize < xCurrentValue;
                XMovementLogic = x => x - 1;
                break;
            case 2:
                XComparisonLogic = xCurrentValue =>
                    startingPoint.X - sxsize < xCurrentValue;
                XMovementLogic = x => x - 1;
                YComparisonLogic = yCurrentValue =>
                    startingPoint.Y - sysize < yCurrentValue;
                YMovementLogic = y => y - 1;
                break;
            case 3:
                (sxsize, sysize) = (sysize, sxsize);
                YComparisonLogic = yCurrentValue =>
                    startingPoint.Y - sysize < yCurrentValue;
                YMovementLogic = y => y - 1;
                break;
        }

        XComparisonLogic ??= xCurrentValue =>
            xCurrentValue < sxsize + startingPoint.X;
        XMovementLogic ??= x => x + 1;
        YComparisonLogic ??= yCurrentValue =>
            yCurrentValue < sysize + startingPoint.Y;
        YMovementLogic ??= y => y + 1;
    }
}