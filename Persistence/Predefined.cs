namespace ShadersTest.Persistence
{
    using Microsoft.Xna.Framework;
    using System.Collections.Generic;

    public static class Predefined
    {
        public static List<Celestial> GetCelestials()
        {
            return new List<Celestial>
            {
                new Celestial()
                {
                    Name = "Land Rivers",
                    ShaderID = "LandRivers",
                    Info = "A terran like planet with land, rivers and clouds.",
                    Parameters = GetLandRiversParams(),
                },
                new Celestial()
                {
                    Name = "Ice World",
                    ShaderID = "IceWorld",
                    Info = "Ice planet, with some water lakes, wind and clouds.",
                    Parameters = GetIceWorldParams(),
                },
                new Celestial()
                {
                    Name = "Terran Dry",
                    ShaderID = "TerranDry",
                    Info = "A mars-like rocky planet, close to its star, dried out of any water.",
                    Parameters = GetTerranDryParams(),
                },
                new Celestial()
                {
                    Name = "Islands",
                    ShaderID = "LandMasses",
                    Info = "Water planets covered in islands.",
                    Parameters = GetLandMassesParams(),
                },
                new Celestial()
                {
                    Name = "No atmosphere",
                    ShaderID = "NoAtmosphere",
                    Info = "Moons or planets not protected by atmosphere.",
                    Parameters = GetNoAtmosphereParams(),
                },
                new Celestial()
                {
                    Name = "Gas Giant I",
                    ShaderID = "GasPlanet",
                    Info = "A cold planet, outside the frost line.",
                    Parameters = GetGasPlanetParams(),
                },
                new Celestial()
                {
                    Name = "Gas Giant II",
                    ShaderID = "GasPlanetLayers",
                    Info = "A cold planet, outside the frost line, variation.",
                    Parameters = GetGasPlanetLayersParams(),
                },
                new Celestial()
                {
                    Name = "Lava World",
                    ShaderID = "LavaWorld",
                    Info = "A protoplanet, perhaps too close to a star.",
                    Parameters = GetLavaWorldParams(),
                },
                new Celestial()
                {
                    Name = "Asteroid",
                    ShaderID = "Asteroid",
                    Info = "Fragment of matter roaming in space.",
                    Parameters = GetAsteroidParams(),
                },
                new Celestial()
                {
                    Name = "Star",
                    ShaderID = "Star",
                    Info = "Huge hydrogen converters.",
                    Parameters = GetStarParams(),
                }
            };
        }

        public static List<Parameter> GetLandRiversParams()
        {
            return new List<Parameter>()
                {
                    Parameter.New("Rotation", "rotation", 0.0f, 0.0f, 0.0f, 360.0f),
                    Parameter.New("Planet time speed", "planet_time_speed", 0.2f, 0.2f, -2.0f, 3.0f),
                    Parameter.New("Planet seed", "planet_seed", 8.98f, 8.98f, 0.0f, 10.0f),
                    Parameter.New("Planet size", "planet_size", 4.6f, 4.6f, 1.0f, 50.0f),
                    Parameter.New("Planet river cutoff", "river_cutoff", 0.368f, 0.368f, 0.0f, 1.0f),
                    Parameter.New("Planet dither size", "dither_size", 2.0f, 2.0f, 0.0f, 10.0f),
                    Parameter.New("Planet light border 1", "light_border_1", 0.4f, 0.4f, 0.0f, 1.0f),
                    Parameter.New("Planet light border 2", "light_border_2", 0.5f, 0.5f, 0.0f, 1.0f),
                    Parameter.New("Planet octaves", "planet_octaves", 6, 6, 0, 20),
                    Parameter.New("Planet color 1", "col1", new Color(0.388f, 0.670f, 0.247f), new Color(0.388f, 0.670f, 0.247f)),
                    Parameter.New("Planet color 2", "col2", new Color(0.231f, 0.490f, 0.309f), new Color(0.231f, 0.490f, 0.309f)),
                    Parameter.New("Planet color 3", "col3", new Color(0.184f, 0.341f, 0.325f), new Color(0.184f, 0.341f, 0.325f)),
                    Parameter.New("Planet color 4", "col4", new Color(0.156f, 0.207f, 0.250f), new Color(0.156f, 0.207f, 0.250f)),
                    Parameter.New("Planet river color", "river_col", new Color(0.309f, 0.643f, 0.721f), new Color(0.309f, 0.643f, 0.721f)),
                    Parameter.New("Planet river color dark", "river_col_dark", new Color(0.250f, 0.286f, 0.450f), new Color(0.250f, 0.286f, 0.450f)),

                    Parameter.New("Clouds cover", "cloud_cover", 0.47f, 0.47f, 0.0f, 1.0f),
                    Parameter.New("Clouds time speed", "time_speed_clouds", 0.1f, 0.1f, -2.0f, 3.0f),
                    Parameter.New("Clouds stretch", "stretch", 2.0f, 2.0f, 1.0f, 3.0f),
                    Parameter.New("Clouds curve", "cloud_curve", 1.3f, 1.3f, 1.0f, 2.0f),
                    Parameter.New("Clouds light border 1", "light_border_clouds_1", 0.52f, 0.52f, 0.0f, 1.0f),
                    Parameter.New("Clouds light border 2", "light_border_clouds_2", 0.62f, 0.62f, 0.0f, 1.0f),
                    Parameter.New("Clouds base color", "base_color", new Color(0.960f, 1.000f, 0.909f), new Color(0.960f, 1.000f, 0.909f)),
                    Parameter.New("Clouds outline color", "outline_color", new Color(0.874f, 0.878f, 0.909f), new Color(0.874f, 0.878f, 0.909f)),
                    Parameter.New("Clouds shadow base color", "shadow_base_color", new Color(0.407f, 0.435f, 0.600f), new Color(0.407f, 0.435f, 0.600f)),
                    Parameter.New("Clouds shadow outline color", "shadow_outline_color", new Color(0.250f, 0.286f, 0.450f), new Color(0.250f, 0.286f, 0.450f)),
                    Parameter.New("Clouds size", "size_clouds", 7.315f, 7.315f, 1.0f, 50.0f),
                    Parameter.New("Clouds octaves", "cloud_octaves", 2, 2, 0, 20),
                    Parameter.New("Clouds seed", "seed_clouds", 5.939f, 5.939f, 1.0f, 10.0f),
                };
        }

        public static List<Parameter> GetIceWorldParams()
        {
            return new List<Parameter>()
                {
                    Parameter.New("Rotation", "rotation", 0.0f, 0.0f, 0.0f, 360.0f),
                    Parameter.New("Planet time speed", "time_speed", 0.2f, 0.2f, -2.0f, 3.0f),
                    Parameter.New("Planet dither size", "dither_size", 2.0f, 2.0f, 0.0f, 10.0f),
                    Parameter.New("Planet light border 1", "light_border_1", 0.4f, 0.4f, 0.0f, 1.0f),
                    Parameter.New("Planet light border 2", "light_border_2", 0.5f, 0.5f, 0.0f, 1.0f),
                    Parameter.New("Planet size", "size", 4.6f, 4.6f, 1.0f, 50.0f),
                    Parameter.New("Planet octaves", "OCTAVES", 6, 6, 0, 20),
                    Parameter.New("Planet seed", "seed", 8.98f, 8.98f, 0.0f, 10.0f),

                    Parameter.New("Lakes cutoff", "lake_cutoff", 0.55f, 0.55f, 0.0f, 1.0f),
                    Parameter.New("Lakes light border 1", "light_border_lake_1", 0.024f, 0.024f, 0.0f, 1.0f),
                    Parameter.New("Lakes light border 2", "light_border_lake_2", 0.047f, 0.047f, 0.0f, 1.0f),
                    Parameter.New("Lakes seed", "seedLakes", 4.14f, 4.14f, 1.0f, 10.0f),
                    Parameter.New("Lakes size", "sizeLakes", 10.0f, 10.0f, 1.0f, 50.0f),

                    Parameter.New("Clouds time speed", "time_speed_clouds", 0.1f, 0.1f, -2.0f, 3.0f),
                    Parameter.New("Clouds cover", "cloud_cover", 0.546f, 0.546f, 0.0f, 1.0f),
                    Parameter.New("Clouds stretch", "stretch", 2.5f, 2.5f, 1.0f, 3.0f),
                    Parameter.New("Clouds curve", "cloud_curve", 1.3f, 1.3f, 1.0f, 2.0f),
                    Parameter.New("Clouds light border 1", "light_border_clouds_1", 0.566f, 0.566f, 0.0f, 1.0f),
                    Parameter.New("Clouds light border 2", "light_border_clouds_2", 0.781f, 0.781f, 0.0f, 1.0f),
                    Parameter.New("Clouds octaves", "OCTAVES_CLOUDS", 4, 4, 0, 20),
                    Parameter.New("Clouds seed", "seedClouds", 1.14f, 1.14f, 1.0f, 10.0f),
                    Parameter.New("Clouds size", "sizeClouds", 4.0f, 4.0f, 1.0f, 50.0f),
                };
        }

        public static List<Parameter> GetTerranDryParams()
        {
            return new List<Parameter>()
                {
                    Parameter.New("Rotation", "rotation", 0.0f, 0.0f, 0.0f, 360.0f),

                    Parameter.New("Light distance 1", "light_distance1", 0.362f, 0.362f, 0.0f, 1.0f),
                    Parameter.New("Light distance 2", "light_distance2", 0.525f, 0.525f, 0.0f, 1.0f),

                    Parameter.New("Time speed", "time_speed", 0.1f, 0.1f, -2.0f, 3.0f),
                    Parameter.New("Dither size", "dither_size", 2.0f, 2.0f, 0.0f, 10.0f),
                    
                    Parameter.New("Size", "size", 8.0f, 8.0f, 1.0f, 50.0f),
                    Parameter.New("Octaves", "OCTAVES", 3, 3, 0, 20),
                    Parameter.New("Seed", "seed", 1.175f, 1.175f, 0.0f, 10.0f),

                };
        }

        public static List<Parameter> GetLandMassesParams()
        {
            return new List<Parameter>()
                {
                    Parameter.New("Rotation", "rotation", 0.0f, 0.0f, 0.0f, 360.0f),
                    Parameter.New("Water time speed", "time_speed", 0.1f, 0.1f, -2.0f, 3.0f),
                    Parameter.New("Water dither size", "dither_size", 2.0f, 2.0f, 0.0f, 10.0f),
                    Parameter.New("Water light border 1", "light_border_1", 0.4f, 0.4f, 0.0f, 1.0f),
                    Parameter.New("Water light border 2", "light_border_2", 0.6f, 0.6f, 0.0f, 1.0f),
                    Parameter.New("Water size", "size", 5.228f, 5.228f, 1.0f, 50.0f),
                    Parameter.New("Water seed", "seed", 10.0f, 10.0f, 0.0f, 10.0f),
                    Parameter.New("Water octaves", "OCTAVES", 3, 3, 0, 20),

                    Parameter.New("Land time speed", "land_time_speed", 0.2f, 0.2f, -2.0f, 3.0f),
                    Parameter.New("Land dither size", "land_dither_size", 2.0f, 2.0f, 0.0f, 10.0f),
                    Parameter.New("Land light border 1", "light_border_land_1", 0.32f, 0.32f, 0.0f, 1.0f),
                    Parameter.New("Land light border 2", "light_border_land_2", 0.534f, 0.534f, 0.0f, 1.0f),
                    Parameter.New("Land cutoff", "land_cutoff", 0.633f, 0.633f, 0.0f, 1.0f),
                    Parameter.New("Land seed", "land_seed", 7.947f, 7.947f, 0.0f, 10.0f),
                    Parameter.New("Land octaves", "land_octaves", 6, 6, 0, 20),
                    Parameter.New("Lakes size", "sizeLakes", 4.292f, 4.292f, 1.0f, 50.0f),

                    Parameter.New("Clouds cover", "cloud_cover", 0.515f, 0.515f, 0.0f, 1.0f),
                    Parameter.New("Clouds time speed", "time_speed_clouds", 0.1f, 0.1f, -2.0f, 3.0f),
                    Parameter.New("Clouds stretch", "stretch", 2.0f, 2.0f, 1.0f, 3.0f),
                    Parameter.New("Clouds curve", "cloud_curve", 1.3f, 1.3f, 1.0f, 2.0f),
                    Parameter.New("Clouds light border 1", "light_border_clouds_1", 0.52f, 0.52f, 0.0f, 1.0f),
                    Parameter.New("Clouds light border 2", "light_border_clouds_2", 0.62f, 0.62f, 0.0f, 1.0f),

                    Parameter.New("Clouds size", "sizeClouds", 7.745f, 7.745f, 1.0f, 50.0f),
                    Parameter.New("Clouds octaves", "OCTAVES_CLOUDS", 2, 2, 0, 20),
                    Parameter.New("Clouds seed", "seedClouds", 5.39f, 5.39f, 1.0f, 10.0f),
                };
        }

        public static List<Parameter> GetNoAtmosphereParams()
        {
            return new List<Parameter>()
                {
                    Parameter.New("Rotation", "rotation", 0.0f, 0.0f, 0.0f, 360.0f),
                    Parameter.New("Planet time speed", "time_speed", 0.4f, 0.4f, -2.0f, 3.0f),
                    Parameter.New("Planet dither size", "dither_size", 2.0f, 2.0f, 0.0f, 10.0f),
                    Parameter.New("Planet light border 1", "light_border_1", 0.615f, 0.615f, 0.0f, 1.0f),
                    Parameter.New("Planet light border 2", "light_border_2", 0.729f, 0.729f, 0.0f, 1.0f),
                    Parameter.New("Planet size", "size", 8.0f, 8.0f, 1.0f, 50.0f),
                    Parameter.New("Planet octaves", "OCTAVES", 4, 4, 0, 20),
                    Parameter.New("Planet seed", "seed", 1.012f, 1.012f, 0.0f, 10.0f),

                    Parameter.New("Crater light border", "light_border_crater", 0.465f, 0.465f, 0.0f, 1.0f),
                    Parameter.New("Crater size", "sizeCraters", 5.0f, 5.0f, 1.0f, 50.0f),
                    Parameter.New("Crater seed", "seedCraters", 4.14f, 4.14f, 1.0f, 10.0f),
                };
        }

        public static List<Parameter> GetGasPlanetParams()
        {
            return new List<Parameter>()
                {
                    Parameter.New("Rotation", "rotation", 0.0f, 0.0f, 0.0f, 360.0f),
                    Parameter.New("Inner clouds time speed", "inner_cloud_time_speed", 0.7f, 0.7f, -2.0f, 3.0f),
                    Parameter.New("Inner clouds cover", "inner_cloud_cover", 0.0f, 0.0f, 0.0f, 1.0f),
                    Parameter.New("Inner clouds stretch", "inner_stretch", 1.0f, 1.0f, 1.0f, 3.0f),
                    Parameter.New("Inner clouds curve", "inner_cloud_curve", 1.3f, 1.3f, 1.0f, 2.0f),
                    Parameter.New("Inner light border 1", "inner_light_border_1", 0.52f, 0.52f, 0.0f, 1.0f),
                    Parameter.New("Inner light border 2", "inner_light_border_2", 0.62f, 0.62f, 0.0f, 1.0f),
                    Parameter.New("Inner size", "inner_size", 9.0f, 9.0f, 1.0f, 50.0f),
                    Parameter.New("Inner seed", "inner_seed", 5.939f, 5.939f, 0.0f, 10.0f),
                    Parameter.New("Inner octaves", "inner_octaves", 5, 5, 0, 20),

                    Parameter.New("Outer clouds time speed", "outer_cloud_time_speed", 0.7f, 0.7f, -2.0f, 3.0f),
                    Parameter.New("Outer clouds cover", "outer_cloud_cover", 0.538f, 0.538f, 0.0f, 1.0f),
                    Parameter.New("Outer clouds stretch", "outer_stretch", 1.0f, 1.0f, 1.0f, 3.0f),
                    Parameter.New("Outer clouds curve", "outer_cloud_curve", 1.3f, 1.3f, 1.0f, 2.0f),
                    Parameter.New("Outer light border 1", "outer_light_border_1", 0.439f, 0.439f, 0.0f, 1.0f),
                    Parameter.New("Outer light border 2", "outer_light_border_2", 0.746f, 0.746f, 0.0f, 1.0f),
                    Parameter.New("Outer size", "outer_size", 9.0f, 9.0f, 1.0f, 50.0f),
                    Parameter.New("Outer seed", "outer_seed", 5.939f, 5.939f, 0.0f, 10.0f),
                    Parameter.New("Outer octaves", "outer_octaves", 5, 5, 0, 20),
                };
        }

        public static List<Parameter> GetGasPlanetLayersParams()
        {
            return new List<Parameter>()
                {
                    Parameter.New("Gas rotation", "gas_rotation", 0.0f, 0.0f, 0.0f, 360.0f),
                    Parameter.New("Gas dither size", "gas_dither_size", 1.0f, 1.0f, 0.0f, 10.0f),
                    Parameter.New("Gas time speed", "gas_time_speed", 0.05f, 0.05f, -2.0f, 3.0f),
                    Parameter.New("Gas bands", "gas_bands", 1.0f, 1.0f, 0.0f, 10.0f),
                    Parameter.New("Gas size", "gas_size", 8.0f, 8.0f, 1.0f, 50.0f),
                    Parameter.New("Gas seed", "gas_seed", 3.036f, 3.036f, 0.0f, 10.0f),
                    Parameter.New("Gas octaves", "gas_octaves", 8, 8, 0, 20),

                    Parameter.New("Ring rotation", "ring_rotation", -0.5f, -0.5f, -360.0f, 360.0f),
                    Parameter.New("Ring time speed", "ring_time_speed", 0.2f, 0.2f, -2.0f, 3.0f),

                    Parameter.New("Outer light border 1", "ring_light_border_1", 0.52f, 0.52f, 0.0f, 1.0f),
                    Parameter.New("Outer light border 2", "ring_light_border_2", 0.62f, 0.62f, 0.0f, 1.0f),

                    Parameter.New("Ring width", "ring_width", 0.127f, 0.127f, 0.0f, 0.30f),

                    Parameter.New("Ring perspective", "ring_perspective", 6.0f, 6.0f, 0.0f, 50f),

                    Parameter.New("Ring scale relative to planet", "ring_scale_rel_to_planet", 6.0f, 6.0f, 0.0f, 50f),

                    Parameter.New("Ring size", "ring_size", 5.0f, 5.0f, 1.0f, 50.0f),
                    Parameter.New("Ring seed", "ring_seed", 8.461f, 8.461f, 0.0f, 10.0f),
                    Parameter.New("Ring octaves", "ring_octaves", 4, 4, 0, 20),

                    Parameter.New("Scale (experimental, buggy)", "fullScale", 1.0f, 1.0f, -10.0f, 10.0f),
                };
        }

        public static List<Parameter> GetLavaWorldParams()
        {
            return new List<Parameter>()
                {
                    Parameter.New("Rotation", "rotation", 0.0f, 0.0f, 0.0f, 360.0f),
                    Parameter.New("Planet time speed", "time_speed", 0.2f, 0.2f, -2.0f, 3.0f),
                    Parameter.New("Planet dither size", "dither_size", 2.0f, 2.0f, 0.0f, 10.0f),
                    Parameter.New("Planet light border 1", "light_border_1", 0.4f, 0.4f, 0.0f, 1.0f),
                    Parameter.New("Planet light border 2", "light_border_2", 0.6f, 0.6f, 0.0f, 1.0f),
                    Parameter.New("Planet size", "size", 10.0f, 10.0f, 1.0f, 50.0f),
                    Parameter.New("Planet octaves", "octaves", 3, 3, 0, 20),
                    Parameter.New("Planet seed", "seed", 1.551f, 1.551f, 0.0f, 10.0f),

                    Parameter.New("Craters time speed", "craters_time_speed", 0.09f, 0.09f, -2.0f, 3.0f),
                    Parameter.New("Craters light border", "craters_light_border", 0.4f, 0.4f, 0.0f, 1.0f),
                    Parameter.New("Crater size", "craters_size", 3.5f, 3.5f, 1.0f, 50.0f),
                    Parameter.New("Crater seed", "craters_seed", 1.561f, 1.561f, 1.0f, 10.0f),

                    Parameter.New("Lava time speed", "lava_time_speed", 0.2f, 0.2f, -2.0f, 3.0f),
                    Parameter.New("Lava light border 1", "lava_light_border_1", 0.019f, 0.019f, 0.0f, 1.0f),
                    Parameter.New("Lava light border 2", "lava_light_border_2", 0.036f, 0.036f, 0.0f, 1.0f),

                    Parameter.New("Lava river cutoff", "lava_river_cutoff", 0.579f, 0.579f, 0.0f, 1.0f),

                    Parameter.New("Lava size", "lava_size", 10.0f, 10.0f, 1.0f, 50.0f),
                    Parameter.New("Lava octaves", "lava_octaves", 4, 4, 0, 20),
                    Parameter.New("Lava seed", "lava_seed", 2.627f, 2.627f, 0.0f, 10.0f),
                };
        }

        public static List<Parameter> GetAsteroidParams()
        {
            return new List<Parameter>()
                {
                    Parameter.New("Rotation", "rotation", 0.0f, 0.0f, 0.0f, 360.0f),

                    Parameter.New("Size", "size", 5.294f, 5.294f, 1.0f, 50.0f),
                    Parameter.New("Octaves", "octaves", 2, 2, 0, 20),
                    Parameter.New("Seed", "seed", 1.567f, 1.567f, 1.0f, 10.0f),
                };
        }

        public static List<Parameter> GetStarParams()
        {
            return new List<Parameter>()
                {
                    Parameter.New("Rotation", "rotation", 0.0f, 0.0f, 0.0f, 360.0f),
                    Parameter.New("Time speed", "star_time_speed", 0.05f, 0.05f, -2.0f, 3.0f),
                    Parameter.New("Size", "star_size", 10.0f, 10.0f, 1.0f, 50.0f),
                    Parameter.New("Tiles", "star_tiles", 1.0f, 1.0f, 0.0f, 20.0f),
                };
        }
    }
}
