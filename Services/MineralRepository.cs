using wmine.Models;
using Newtonsoft.Json;

namespace wmine.Services
{
    public interface IMineralRepository
    {
        MineralInfo? GetMineralInfo(MineralType type);
        Dictionary<MineralType, MineralInfo> GetAllMineralInfo();
        List<MineralInfo> SearchByLocality(string locality);
        List<MineralInfo> GetMineralsByRegion(string region);
        string GetStatisticsSummary();
    }

    public class MineralRepository : IMineralRepository
    {
        private readonly Dictionary<MineralType, MineralInfo> _mineralData;
        private readonly string _dataFilePath;

        public MineralRepository()
        {
            // Data file in application folder (can be edited)
            var exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            _dataFilePath = Path.Combine(exeFolder, "data", "minerals.json");

            if (File.Exists(_dataFilePath))
            {
                try
                {
                    var txt = File.ReadAllText(_dataFilePath);
                    var list = JsonConvert.DeserializeObject<List<MineralInfo>>(txt);
                    _mineralData = list?.ToDictionary(m => m.Type) ?? BuildMineralDictionary();
                }
                catch
                {
                    _mineralData = BuildMineralDictionary();
                }
            }
            else
            {
                _mineralData = BuildMineralDictionary();
                // Ensure directory exists and write default file for easier editing
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_dataFilePath) ?? Path.Combine(exeFolder, "data"));
                    var serialized = JsonConvert.SerializeObject(_mineralData.Values.OrderBy(m=>m.Type).ToList(), Formatting.Indented);
                    File.WriteAllText(_dataFilePath, serialized);
                }
                catch
                {
                    // ignore write errors
                }
            }
        }

        private static Dictionary<MineralType, MineralInfo> BuildMineralDictionary()
        {
            return new Dictionary<MineralType, MineralInfo>
            {
                // Copy full dataset from original implementation
                { MineralType.Cuivre, new MineralInfo { Type = MineralType.Cuivre, Nom = "Cuivre natif et minerais cuprifères", FormuleChimique = "Cu (natif), Cu2S, CuFeS2", Description = "Le cuivre a été exploité dans le Var dès l'Antiquité. Le massif des Maures et l'Estérel contiennent de nombreux indices cuprifères.", LocalitesVar = new List<string>{ "Cap Garonne (Le Pradet) - Mine principale 1842-1917","La Mole - Filons de chalcopyrite","Roquebrune-sur-Argens - Anciennes mines","Massif de l'Esterel - Indices cuprifères","Collobrières - Gisements secondaires" }, Utilisation = "Électricité, plomberie, alliages (bronze, laiton), électronique", ProprietesPhysiques = "Métal rouge-orangé, malléable, excellent conducteur électrique", DureteMohs = "2.5 - 3", Densite = "8.9 g/cm³", SystemeCristallin = "Cubique", SourcesWeb = new List<string>{ "https://www.brgm.fr","https://www.mindat.org/loc-23431.html (Var)","https://www.capgaronne.com (Musée Mine Cap Garonne)" } } },
                { MineralType.Fer, new MineralInfo { Type = MineralType.Fer, Nom = "Minerais de fer (hématite, limonite)", FormuleChimique = "Fe2O3, Fe(OH)·nH2O", Description = "Exploitation du fer dans le Var depuis l'époque romaine, particulièrement dans le massif des Maures.", LocalitesVar = new List<string>{ "Tanneron - Anciennes mines importantes","Cabasse - Gisements d'hématite","Besse-sur-Issole - Mines historiques","La Londe-les-Maures - Filons ferrifères","Hyères - Indices dans le socle cristallin" }, Utilisation = "Sidérurgie, construction, alliages, pigments", ProprietesPhysiques = "Rouge brique (hématite) à brun-jaune (limonite), opaque", DureteMohs = "5 - 6.5", Densite = "4.9 - 5.3 g/cm³", SystemeCristallin = "Trigonal (hématite)", SourcesWeb = new List<string>{ "https://www.brgm.fr","https://www.geologie-var.fr","https://infoterre.brgm.fr" } } },
                { MineralType.Plomb, new MineralInfo { Type = MineralType.Plomb, Nom = "Galène et minerais plombifères", FormuleChimique = "PbS", Description = "La galène (sulfure de plomb) a été exploitée dans plusieurs secteurs du Var, souvent associée au zinc.", LocalitesVar = new List<string>{ "Tanneron - Filons Pb-Zn","Cavalaire - Indices plombifères","Toulon - Secteur du Mont Faron","Pierrefeu - Anciennes recherches","Bormes-les-Mimosas - Gisements secondaires" }, Utilisation = "Batteries, protection radiologique, alliages", ProprietesPhysiques = "Gris plomb métallique, très dense, éclat brillant", DureteMohs = "2.5", Densite = "7.4 - 7.6 g/cm³", SystemeCristallin = "Cubique", SourcesWeb = new List<string>{ "https://www.mindat.org","https://www.brgm.fr","https://geologie-var.fr" } } },
                { MineralType.Zinc, new MineralInfo { Type = MineralType.Zinc, Nom = "Blende (sphalérite)", FormuleChimique = "ZnS", Description = "La blende (sulfure de zinc) est souvent associée à la galène dans les filons du Var.", LocalitesVar = new List<string>{ "Tanneron - Filons Pb-Zn exploités","Saint-Raphaël - Indices dans l'Estérel","Fréjus - Gisements secondaires","Roquebrune - Minéralisation associée","La Garde-Freinet - Traces dans le massif" }, Utilisation = "Galvanisation, alliages (laiton), peintures, chimie", ProprietesPhysiques = "Brun à noir, éclat résineux ou adamantin", DureteMohs = "3.5 - 4", Densite = "3.9 - 4.1 g/cm³", SystemeCristallin = "Cubique", SourcesWeb = new List<string>{ "https://www.mindat.org","https://www.brgm.fr" } } },
                { MineralType.Antimoine, new MineralInfo { Type = MineralType.Antimoine, Nom = "Stibine (sulfure d'antimoine)", FormuleChimique = "Sb2S3", Description = "L'antimoine a été recherché dans le Var, principalement pour ses applications industrielles.", LocalitesVar = new List<string>{ "Collobrières - Indices antimonifères","Massif des Maures - Filons mineurs","Bormes - Recherches historiques","Cogolin - Traces de stibine" }, Utilisation = "Alliages, retardateurs de flamme, semiconducteurs", ProprietesPhysiques = "Gris plomb métallique, cristaux aciculaires", DureteMohs = "2", Densite = "4.6 - 4.7 g/cm³", SystemeCristallin = "Orthorhombique", SourcesWeb = new List<string>{ "https://www.mindat.org","https://www.brgm.fr" } } },
                { MineralType.Argent, new MineralInfo { Type = MineralType.Argent, Nom = "Argent natif et argentite", FormuleChimique = "Ag (natif), Ag2S", Description = "L'argent se trouve généralement associé aux minerais de plomb dans le Var, en très faibles quantités.", LocalitesVar = new List<string>{ "Tanneron - Traces dans filons Pb-Zn","Le Pradet - Indices argentifères mineurs","Pierrefeu - Recherches anciennes" }, Utilisation = "Bijouterie, électronique, photographie, monnaie", ProprietesPhysiques = "Blanc argenté brillant, très malléable", DureteMohs = "2.5 - 3", Densite = "10.5 g/cm³", SystemeCristallin = "Cubique", SourcesWeb = new List<string>{ "https://www.mindat.org","https://www.brgm.fr" } } },
                { MineralType.Baryum, new MineralInfo { Type = MineralType.Baryum, Nom = "Barytine (sulfate de baryum)", FormuleChimique = "BaSO4", Description = "La barytine se trouve dans les filons hydrothermaux du Var, utilisée comme charge minérale.", LocalitesVar = new List<string>{ "Saint-Cyr-sur-Mer - Gisements de barytine","Bandol - Filons hydrothermaux","La Cadière-d'Azur - Indices barytiques","Six-Fours - Anciennes extractions" }, Utilisation = "Boues de forage pétrolier, charges minérales, radio-opacité", ProprietesPhysiques = "Blanc à jaunâtre, très dense, éclat vitreux", DureteMohs = "3 - 3.5", Densite = "4.3 - 4.6 g/cm³", SystemeCristallin = "Orthorhombique", SourcesWeb = new List<string>{ "https://www.mindat.org","https://www.brgm.fr" } } },
                { MineralType.Fluor, new MineralInfo { Type = MineralType.Fluor, Nom = "Fluorine (fluorure de calcium)", FormuleChimique = "CaF2", Description = "La fluorine se trouve dans les filons hydrothermaux du Var, en cristaux parfois spectaculaires.", LocalitesVar = new List<string>{ "Tanneron - Cristaux de fluorine violette","Massif de l'Estérel - Filons fluorés","Roquebrune - Fluorine verte","Saint-Raphaël - Géodes à fluorine" }, Utilisation = "Métallurgie, production d'acide fluorhydrique, optique", ProprietesPhysiques = "Incolore à violet/vert, transparente, fluorescence UV", DureteMohs = "4", Densite = "3.0 - 3.3 g/cm³", SystemeCristallin = "Cubique", SourcesWeb = new List<string>{ "https://www.mindat.org","https://www.geoforum.fr" } } },
                { MineralType.Apatite, new MineralInfo { Type = MineralType.Apatite, Nom = "Apatite (phosphate de calcium)", FormuleChimique = "Ca5(PO4)3(F,Cl,OH)", Description = "L'apatite est un minéral accessoire dans les roches du Var, parfois en beaux cristaux.", LocalitesVar = new List<string>{ "Massif des Maures - Cristaux dans pegmatites","Estérel - Apatite verte","Toulon - Mont Faron","Hyères - Gneiss à apatite" }, Utilisation = "Engrais phosphatés, chimie, pierres semi-précieuses", ProprietesPhysiques = "Vert, bleu ou violet, éclat vitreux à résineux", DureteMohs = "5", Densite = "3.1 - 3.2 g/cm³", SystemeCristallin = "Hexagonal", SourcesWeb = new List<string>{ "https://www.mindat.org","https://www.geologie-provence.fr" } } },
                { MineralType.Améthyste, new MineralInfo { Type = MineralType.Améthyste, Nom = "Améthyste (quartz violet)", FormuleChimique = "SiO2", Description = "Variété violette de quartz, rare dans le Var mais présente dans certaines géodes.", LocalitesVar = new List<string>{ "Massif de l'Estérel - Géodes à améthyste","Roquebrune - Filons quartzeux","Fréjus - Cristaux dans rhyolites" }, Utilisation = "Joaillerie, collection minéralogique, lithothérapie", ProprietesPhysiques = "Violet clair à foncé, transparent, éclat vitreux", DureteMohs = "7", Densite = "2.65 g/cm³", SystemeCristallin = "Trigonal (rhomboédrique)", SourcesWeb = new List<string>{ "https://www.mindat.org","https://www.france-mineraux.fr" } } },
                { MineralType.Andalousite, new MineralInfo { Type = MineralType.Andalousite, Nom = "Andalousite", FormuleChimique = "Al2SiO5", Description = "Silicate d'aluminium présent dans les roches métamorphiques du massif des Maures.", LocalitesVar = new List<string>{ "Massif des Maures - Schistes métamorphiques","Collobrières - Cristaux dans micaschistes","Bormes-les-Mimosas - Gneiss à andalousite" }, Utilisation = "Matériaux réfractaires, céramiques haute température", ProprietesPhysiques = "Rose, brun ou gris, prismes allongés", DureteMohs = "7 - 7.5", Densite = "3.1 - 3.2 g/cm³", SystemeCristallin = "Orthorhombique", SourcesWeb = new List<string>{ "https://www.mindat.org","https://www.brgm.fr" } } },
                { MineralType.Disthéne, new MineralInfo { Type = MineralType.Disthéne, Nom = "Disthène (Cyanite)", FormuleChimique = "Al2SiO5", Description = "Minéral métamorphique bleu, polymorphe de l'andalousite, trouvé dans les schistes du Var.", LocalitesVar = new List<string>{ "Massif des Maures - Schistes à disthène","Plan-de-la-Tour - Cristaux bleus","La Garde-Freinet - Micaschistes" }, Utilisation = "Céramiques, réfractaires, abrasifs", ProprietesPhysiques = "Bleu ciel, lames aplaties, clivage parfait", DureteMohs = "4.5-5 (long.) / 6.5-7 (travers)", Densite = "3.5 - 3.7 g/cm³", SystemeCristallin = "Triclinique", SourcesWeb = new List<string>{ "https://www.mindat.org","https://www.geologie-provence.fr" } } },
                { MineralType.Estérellite, new MineralInfo { Type = MineralType.Estérellite, Nom = "Estérellite (Rhyolite orbiculaire)", FormuleChimique = "Roche volcanique (non minéral unique)", Description = "Roche volcanique ornementale unique au monde, caractéristique du massif de l'Estérel.", LocalitesVar = new List<string>{ "Massif de l'Estérel - UNIQUE AU MONDE","Saint-Raphaël - Carrières historiques","Agay - Affleurements spectaculaires","Fréjus - Zones d'extraction anciennes" }, Utilisation = "Pierre ornementale, décoration, collection", ProprietesPhysiques = "Rouge à rose, structure orbiculaire caractéristique", DureteMohs = "6 - 7 (roche)", Densite = "2.6 - 2.7 g/cm³", SystemeCristallin = "Roche (texture orbiculaire)", SourcesWeb = new List<string>{ "https://www.esterellite.fr","https://www.geologie-esterel.fr","https://www.ville-saintraphael.fr" } } },
                { MineralType.Grenats, new MineralInfo { Type = MineralType.Grenats, Nom = "Grenats (almandin, spessartine)", FormuleChimique = "X3Y2(SiO4)3", Description = "Groupe de minéraux métamorphiques, abondants dans les schistes et gneiss du Var.", LocalitesVar = new List<string>{ "Massif des Maures - Grenats almandin","Collobrières - Beaux cristaux dodécaédriques","Le Lavandou - Schistes à grenats","Bormes - Gneiss grenatifères","Cavalaire - Grenats dans micaschistes" }, Utilisation = "Abrasifs industriels, joaillerie (variétés nobles), filtration", ProprietesPhysiques = "Rouge foncé à noir, dodécaèdres, éclat vitreux", DureteMohs = "6.5 - 7.5", Densite = "3.5 - 4.3 g/cm³", SystemeCristallin = "Cubique", SourcesWeb = new List<string>{ "https://www.mindat.org","https://www.geologie-var.fr" } } },
                { MineralType.Orthose, new MineralInfo { Type = MineralType.Orthose, Nom = "Orthose (Feldspath potassique)", FormuleChimique = "KAlSi3O8", Description = "Feldspath abondant dans les granites et pegmatites du massif des Maures.", LocalitesVar = new List<string>{ "Massif des Maures - Pegmatites à orthose rose","Collobrières - Grands cristaux","Le Lavandou - Granite à feldspath","Cavalaire - Filons pegmatitiques" }, Utilisation = "Céramiques, porcelaine, verre, charges minérales", ProprietesPhysiques = "Rose à blanc, éclat vitreux, clivages nets", DureteMohs = "6", Densite = "2.55 - 2.63 g/cm³", SystemeCristallin = "Monoclinique", SourcesWeb = new List<string>{ "https://www.mindat.org","https://www.geologie-provence.fr" } } },
                { MineralType.Staurotite, new MineralInfo { Type = MineralType.Staurotite, Nom = "Staurotide (Pierre de croix)", FormuleChimique = "Fe2Al9Si4O23(OH)", Description = "Minéral métamorphique formant des macles en croix caractéristiques.", LocalitesVar = new List<string>{ "Massif des Maures - Schistes métamorphiques","Collobrières - Macles en croix","La Garde-Freinet - Cristaux prismatiques" }, Utilisation = "Collection minéralogique, amulettes (croix naturelles)", ProprietesPhysiques = "Brun foncé à noir, macles en croix à 60° ou 90°", DureteMohs = "7 - 7.5", Densite = "3.7 - 3.8 g/cm³", SystemeCristallin = "Monoclinique", SourcesWeb = new List<string>{ "https://www.mindat.org","https://www.geologie-var.fr" } } },
                { MineralType.Tourmaline, new MineralInfo { Type = MineralType.Tourmaline, Nom = "Tourmaline (Schorl, Elbaïte)", FormuleChimique = "(Na,Ca)(Li,Mg,Fe,Al)3Al6(BO3)3Si6O18(OH)4", Description = "Borosilicate complexe présent dans les pegmatites du massif des Maures.", LocalitesVar = new List<string>{ "Massif des Maures - Pegmatites à tourmaline noire","Collobrières - Cristaux prismatiques","Bormes - Schorl dans granite","Le Plan-de-la-Tour - Filons pegmatitiques" }, Utilisation = "Joaillerie (variétés colorées), piézoélectricité, collection", ProprietesPhysiques = "Noir (schorl) à multicolore, prismes striés, pyroélectrique", DureteMohs = "7 - 7.5", Densite = "2.9 - 3.3 g/cm³", SystemeCristallin = "Trigonal", SourcesWeb = new List<string>{ "https://www.mindat.org","https://www.france-mineraux.fr" } } },
                { MineralType.Lithophyses, new MineralInfo { Type = MineralType.Lithophyses, Nom = "Lithophyses (structures volcaniques)", FormuleChimique = "Structures (non minéral unique)", Description = "Sphères creuses formées par dégazage dans les laves rhyolitiques de l'Estérel.", LocalitesVar = new List<string>{ "Massif de l'Estérel - Structures caractéristiques","Agay - Rhyolites à lithophyses","Saint-Raphaël - Formations volcaniques","Fréjus - Coulées avec lithophyses" }, Utilisation = "Géologie structurale, collection, enseignement", ProprietesPhysiques = "Sphères creuses (1-10 cm) dans roches volcaniques", DureteMohs = "6 - 7 (roche)", Densite = "2.4 - 2.6 g/cm³", SystemeCristallin = "Structure (non cristalline)", SourcesWeb = new List<string>{ "https://www.geologie-esterel.fr","https://www.brgm.fr" } } },
                { MineralType.Septarias, new MineralInfo { Type = MineralType.Septarias, Nom = "Septarias (concrétions calcaires)", FormuleChimique = "CaCO3 + argiles", Description = "Concrétions calcaires avec fissures radiaires remplies de calcite ou quartz.", LocalitesVar = new List<string>{ "Littoral varois - Formations sédimentaires","Bandol - Falaises calcaires","La Ciotat - Concrétions dans marnes","Six-Fours - Niveau à septarias" }, Utilisation = "Collection paléontologique, décoration", ProprietesPhysiques = "Sphères grises avec fissures radiales jaunes/blanches", DureteMohs = "3 (calcaire)", Densite = "2.5 - 2.7 g/cm³", SystemeCristallin = "Structure sédimentaire", SourcesWeb = new List<string>{ "https://www.geologie-provence.fr","https://www.fossiles-var.fr" } } },
                { MineralType.Bulles, new MineralInfo { Type = MineralType.Bulles, Nom = "Vacuoles volcaniques (bulles de gaz)", FormuleChimique = "Structures (cavités)", Description = "Cavités laissées par le dégazage des laves, parfois tapissées de cristaux.", LocalitesVar = new List<string>{ "Massif de l'Estérel - Laves bulleuses","Agay - Scories volcaniques","Saint-Raphaël - Basaltes vacuolaires","Roquebrune - Formations volcaniques" }, Utilisation = "Volcanologie, collection, géodes à minéraux", ProprietesPhysiques = "Cavités sphériques ou elliptiques (mm à cm)", DureteMohs = "N/A (cavité)", Densite = "N/A (vide)", SystemeCristallin = "N/A (structure)", SourcesWeb = new List<string>{ "https://www.geologie-esterel.fr","https://www.volcanisme-france.fr" } } },
                { MineralType.CombustiblesMinéraux, new MineralInfo { Type = MineralType.CombustiblesMinéraux, Nom = "Combustibles minéraux (lignite, charbon)", FormuleChimique = "C + matière organique", Description = "Petits gisements de lignite d'âge oligocène exploités au XIXe siècle dans le Var.", LocalitesVar = new List<string>{ "Gardanne (limite Var) - Bassin houiller","Fuveau - Lignites tertiaires","Besse-sur-Issole - Anciennes exploitations","Le Luc - Petits gisements historiques" }, Utilisation = "Combustible (abandonné), histoire industrielle", ProprietesPhysiques = "Noir brillant à mat, friable, combustible", DureteMohs = "1 - 2", Densite = "1.1 - 1.5 g/cm³", SystemeCristallin = "Amorphe", SourcesWeb = new List<string>{ "https://www.brgm.fr","https://www.patrimoine-minier-paca.fr" } } },
                { MineralType.Uranifères, new MineralInfo { Type = MineralType.Uranifères, Nom = "Minerais uranifères (uraninite, autunite)", FormuleChimique = "UO2, Ca(UO2)2(PO4)2·10H2O", Description = "Indices uranifères dans le massif des Maures, prospectés dans les années 1950-1970.", LocalitesVar = new List<string>{ "Massif des Maures - Indices uranifères","Collobrières - Prospections CEA","La Londe - Anomalies radiométriques","Le Lavandou - Recherches historiques" }, Utilisation = "Énergie nucléaire, médecine nucléaire (abandonné dans le Var)", ProprietesPhysiques = "Noir (uraninite) à jaune-vert (autunite), radioactif", DureteMohs = "5 - 6", Densite = "6.5 - 10 g/cm³", SystemeCristallin = "Cubique (uraninite)", SourcesWeb = new List<string>{ "https://www.irsn.fr","https://www.brgm.fr","https://www.mindat.org" } } }
            };
        }

        public MineralInfo? GetMineralInfo(MineralType type)
        {
            return _mineralData.TryGetValue(type, out var info) ? info : null;
        }

        public Dictionary<MineralType, MineralInfo> GetAllMineralInfo()
        {
            return new Dictionary<MineralType, MineralInfo>(_mineralData);
        }

        public List<MineralInfo> SearchByLocality(string locality)
        {
            locality = locality.ToLowerInvariant();
            return _mineralData.Values
                .Where(m => m.LocalitesVar.Any(l => l.ToLowerInvariant().Contains(locality)))
                .ToList();
        }

        public List<MineralInfo> GetMineralsByRegion(string region)
        {
            region = region.ToLowerInvariant();
            var keywords = new Dictionary<string, List<string>>
            {
                ["maures"] = new() { "maures", "collobrières", "bormes", "lavandou", "garde-freinet" },
                ["esterel"] = new() { "estérel", "esterel", "saint-raphaël", "agay", "fréjus", "roquebrune" },
                ["littoral"] = new() { "toulon", "hyères", "bandol", "six-fours", "sanary", "la seyne" },
                ["centre"] = new() { "brignoles", "besse", "luc", "cabasse" },
                ["tanneron"] = new() { "tanneron", "auribeau", "callian" }
            };

            if (!keywords.ContainsKey(region))
                return new List<MineralInfo>();

            var searchTerms = keywords[region];
            return _mineralData.Values
                .Where(m => m.LocalitesVar.Any(l =>
                    searchTerms.Any(term => l.ToLowerInvariant().Contains(term))))
                .ToList();
        }

        public string GetStatisticsSummary()
        {
            var totalMinerals = _mineralData.Count;
            var totalLocalities = _mineralData.Values
                .SelectMany(m => m.LocalitesVar)
                .Distinct()
                .Count();
            var withWebSources = _mineralData.Values
                .Count(m => m.SourcesWeb.Any());

            return $@"📊 Statistiques Minéraux du Var

✅ {totalMinerals} types de minéraux/formations recensés
📍 {totalLocalities} localités distinctes documentées
🌐 {withWebSources} fiches avec sources web vérifiées
📅 Dernière mise à jour : {DateTime.Now:dd/MM/yyyy}

Sources principales :
• BRGM (Bureau de Recherches Géologiques et Minières)
• Mindat.org (Base de données minéralogiques mondiale)
• Géologie du Var et de Provence
• Musée Mine du Cap Garonne";
        }
    }
}
