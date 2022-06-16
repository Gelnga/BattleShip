using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BattleShipBrain;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class BattleShipDbContext : DbContext
    {
        private static string ConnectionString =
            "Host=localhost;Port=5436;Username=postgres;Password=postgres;database=BattleShip";
        
        public DbSet<SavedBsBrain> SavedBsBrains { get; set; } = default!;
        public DbSet<BsBrainConfiguration> BsBrainConfigurations { get; set; } = default!;
        public DbSet<ShipInBSBrainConfiguration> ShipInBSBrainConfigurations { get; set; } = default!;
        public DbSet<ShipConfiguration> ShipConfigurations { get; set; } = default!;
        public DbSet<ShipPlacementState> ShipPlacementStates { get; set; } = default!;


        public BattleShipDbContext()
        {
        }
        public BattleShipDbContext(DbContextOptions<BattleShipDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // remove the cascade delete globally
            foreach (var relationship in modelBuilder.Model
                .GetEntityTypes()
                .Where(e => !e.IsOwned())
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        public void DeleteBattleshipSaveFileById(int id)
        {
            var saveToDelete = SavedBsBrains.First(brain => brain.Id == id);
            var brainConfId = SavedBsBrains.First(brain => brain.Id == id).BsBrainConfigurationId;
            var shipConfInBrainConf = ShipInBSBrainConfigurations
                .Where(sConfInBConf => sConfInBConf.BsBrainConfigurationId == brainConfId)
                .ToList();

            SavedBsBrains.Remove(saveToDelete);

            foreach (var sConfInBConf in shipConfInBrainConf)
            {
                ShipInBSBrainConfigurations.Remove(sConfInBConf);
                
                var sConf = new ShipConfiguration {Id = sConfInBConf.ShipConfigurationId};
                ShipConfigurations.Attach(sConf);
                ShipConfigurations.Remove(sConf);
            }
            
            var brainConfLocal = new BsBrainConfiguration {Id = brainConfId};
            BsBrainConfigurations.Attach(brainConfLocal);
            BsBrainConfigurations.Remove(brainConfLocal);

            SaveChanges();
        }

        public BsBrain LoadBsBrainById(int id)
        {
            var brainToLoad = SavedBsBrains.First(brain => brain.Id == id);
            return LoadBsBrainBySaveName(brainToLoad.SavedBrainName);
        }
        public BsBrain LoadBsBrainBySaveName(string saveName)
        {
            var loadedBsBrain = SavedBsBrains.First(savedGame =>
                savedGame.SavedBrainName == saveName);

            var loadedConf = LoadBsBrainConfigByBsBrainId(loadedBsBrain.Id);
            
            var loadedBrain = new BsBrain(loadedConf);
            loadedBrain.RestoreBrainFromDb(loadedBsBrain);

            return loadedBrain;
        }

        public GameConfig LoadBsBrainConfigByBsBrainSaveName(string saveName)
        {
            var loadedBrainId = SavedBsBrains.First(brain => brain.SavedBrainName == saveName).Id;
            return LoadBsBrainConfigByBsBrainId(loadedBrainId);
        }

        public GameConfig LoadBsBrainConfigByBsBrainId(int id)
        {
            var configId = SavedBsBrains.First(brain => brain.Id == id).BsBrainConfigurationId;
            
            var savedBrainConf = BsBrainConfigurations.First(configuration =>
                configuration.Id == configId);

            var parsedTouchRule = Enum.TryParse(savedBrainConf.EShipTouchRule, out EShipTouchRule touchRule)
                ? touchRule
                : EShipTouchRule.NoTouch;

            var loadedConf = new GameConfig
            {
                BoardWidth = savedBrainConf.BoardWidth,
                BoardLength = savedBrainConf.BoardLength,
                EShipTouchRule = parsedTouchRule,
                ShipConfigs = new List<ShipConfig>()
            };
            
            var listOfShipConfigsIds = ShipInBSBrainConfigurations
                .Where(shipConfInBrainConf => shipConfInBrainConf.BsBrainConfigurationId == savedBrainConf.Id)
                .Select(shipInBsBrainConfiguration => shipInBsBrainConfiguration.ShipConfigurationId)
                .ToList();
            
            foreach (var shipConfId in listOfShipConfigsIds)
            {
                var loadedSConf = ShipConfigurations.First(sConf =>
                    sConf.Id == shipConfId);

                var objSConf = new ShipConfig
                {
                    Name = loadedSConf.ShipName,
                    Quantity = loadedSConf.ShipQuantity,
                    ShipSizeX = loadedSConf.ShipSizeX,
                    ShipSizeY = loadedSConf.ShipSizeY
                };
                
                loadedConf.ShipConfigs.Add(objSConf);
            }

            return loadedConf;
        } 

        public void SaveBsBrainToDb(BsBrain brain, GameConfig configuration, string saveName)
        {
            var gameConfigDto = new BsBrainConfiguration
            {
                BoardLength = configuration.BoardLength,
                BoardWidth = configuration.BoardWidth,
                EShipTouchRule = configuration.EShipTouchRule.ToString()
            };

            BsBrainConfigurations.Add(gameConfigDto);
            configuration.ShipConfigs.Reverse();

            foreach (var shipConfig in configuration.ShipConfigs)
            {
                var sConfigDto = new ShipConfiguration
                {
                    ShipName = shipConfig.Name,
                    ShipQuantity = shipConfig.Quantity,
                    ShipSizeX = shipConfig.ShipSizeX,
                    ShipSizeY = shipConfig.ShipSizeY
                }; 
            
                ShipConfigurations.Add(sConfigDto);
            }
            
            configuration.ShipConfigs.Reverse();
            
            SaveChanges();

            var lastAddedShipConfId = ShipConfigurations
                    .OrderByDescending(sConfiguration => sConfiguration.Id)
                    .FirstOrDefault()!.Id;
            var lastAddedBsBrainConfId = BsBrainConfigurations
                .OrderByDescending(bsBConfiguration => bsBConfiguration.Id)
                .FirstOrDefault()!.Id;

            var shipConfigsAmount = configuration.ShipConfigs.Count;

            while (shipConfigsAmount != 0)
            {
                var shipConfigBsBrainConfigLink = new ShipInBSBrainConfiguration
                {
                    BsBrainConfigurationId = lastAddedBsBrainConfId,
                    ShipConfigurationId = lastAddedShipConfId
                };
                ShipInBSBrainConfigurations.Add(shipConfigBsBrainConfigLink);

                lastAddedShipConfId--;
                shipConfigsAmount--;
            }

            var savedBsBrain = new SavedBsBrain
            {
                GameBoards = brain.GetBrainGameBoardsJson(false),
                SavedBrainName = saveName,
                AgainstAi = brain.IsGameAgainstAi(),
                BsBrainConfigurationId = lastAddedBsBrainConfId,
                CurrentPlayerId = brain.GetCurrentPlayerId()
            };

            Add(savedBsBrain);
            SaveChanges();
        }

        public void SaveBrainShipPlacementStateByBrainName(string brainName)
        {
            var loadedBrain = LoadBsBrainBySaveName(brainName);
            var loadedBrainId = SavedBsBrains.First(brain => brain.SavedBrainName == brainName).Id;

            var shipPlacementState = loadedBrain.GetBrainShipPlacementState();
            shipPlacementState.SavedBsBrainId = loadedBrainId;
            ShipPlacementStates.Add(shipPlacementState);
            SaveChanges();
        }

        public ShipPlacementState LoadBrainShipPlacementStateByBrainName(string brainName)
        {
            var loadedBrainId = SavedBsBrains.First(brain => brain.SavedBrainName == brainName).Id;

            var shipPlacementState = ShipPlacementStates.First(state => state.SavedBsBrainId == loadedBrainId);
            return shipPlacementState;
        }

        public void UpdateExistingBrain(string gId, BsBrain bsBrain, bool includeShipPlacementStateUpdate)
        {
            var loadedBrain = SavedBsBrains.First(brain => brain.SavedBrainName == gId);

            loadedBrain.GameBoards = bsBrain.GetBrainGameBoardsJson();
            loadedBrain.CurrentPlayerId = bsBrain.GetCurrentPlayerId();

            if (includeShipPlacementStateUpdate)
            {
                var loadedBrainId = SavedBsBrains.First(brain => brain.SavedBrainName == gId).Id;
                var loadedShipPlacement = ShipPlacementStates.First(state => state.SavedBsBrainId == loadedBrainId);
                var currentShipPlacement = bsBrain.GetBrainShipPlacementState();

                loadedShipPlacement.Rotation = currentShipPlacement.Rotation;
                loadedShipPlacement.CurrentShipQuantity = currentShipPlacement.CurrentShipQuantity;
                loadedShipPlacement.PreviouslyPlacedShips = currentShipPlacement.PreviouslyPlacedShips;
                loadedShipPlacement.ShipConfigIndex = currentShipPlacement.ShipConfigIndex;
                loadedShipPlacement.ShipPlacementPreviewCoordinate = currentShipPlacement.ShipPlacementPreviewCoordinate;
            }

            SaveChanges();
        }

        public override int SaveChanges()
        {
            FixEntitiesDateTime(this);
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            FixEntitiesDateTime(this);
            return base.SaveChangesAsync(cancellationToken);
        }

        private void FixEntitiesDateTime(BattleShipDbContext context)
        {
            var dateProperties = context.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(DateTime))
                .Select(z => new
                {
                    ParentName = z.DeclaringEntityType.Name,
                    PropertyName = z.Name
                });

            var editedEntitiesInTheDbContextGraph = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(x => x.Entity);

            foreach (var entity in editedEntitiesInTheDbContextGraph)
            {
                var entityFields = dateProperties.Where(d => d.ParentName == entity.GetType().FullName);

                foreach (var property in entityFields)
                {
                    var prop = entity.GetType().GetProperty(property.PropertyName);

                    if (prop == null)
                        continue;

                    var originalValue = prop.GetValue(entity) as DateTime?;
                    if (originalValue == null)
                        continue;

                    prop.SetValue(entity, DateTime.SpecifyKind(originalValue.Value, DateTimeKind.Utc));
                }
            }
        }
    }
}