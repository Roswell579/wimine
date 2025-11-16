using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using wmine.Models;

namespace wmine.Services
{
    /// <summary>
    /// Service d'export de données GPS au format GPX
    /// Compatible avec Garmin, smartphones, et tous les appareils GPS
    /// </summary>
    public class GpxExportService
    {
        private const string GPX_NAMESPACE = "http://www.topografix.com/GPX/1/1";
        private const string GPX_VERSION = "1.1";
        private const string CREATOR = "WMine - Localisateur de Filons Miniers";
        
        /// <summary>
        /// Exporte une liste de filons au format GPX (waypoints)
        /// </summary>
        public bool ExportFilonsToGpx(List<Filon> filons, string filePath)
        {
            try
            {
                // Filtrer les filons avec coordonnées GPS valides
                var validFilons = filons.Where(f => 
                    f.Latitude.HasValue && 
                    f.Longitude.HasValue &&
                    f.Latitude.Value >= -90 && f.Latitude.Value <= 90 &&
                    f.Longitude.Value >= -180 && f.Longitude.Value <= 180).ToList();
                
                if (validFilons.Count == 0)
                {
                    throw new InvalidOperationException("Aucun filon avec coordonnées GPS valides é exporter.");
                }
                
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    Encoding = Encoding.UTF8
                };
                
                using (var writer = XmlWriter.Create(filePath, settings))
                {
                    writer.WriteStartDocument();
                    
                    // Root GPX element
                    writer.WriteStartElement("gpx", GPX_NAMESPACE);
                    writer.WriteAttributeString("version", GPX_VERSION);
                    writer.WriteAttributeString("creator", CREATOR);
                    
                    // Metadata
                    writer.WriteStartElement("metadata");
                    writer.WriteElementString("name", "Filons Miniers - Export WMine");
                    writer.WriteElementString("desc", $"Export de {validFilons.Count} filon(s) minier(s)");
                    writer.WriteElementString("time", DateTime.UtcNow.ToString("o"));
                    writer.WriteEndElement(); // metadata
                    
                    // Waypoints (un par filon)
                    foreach (var filon in validFilons)
                    {
                        WriteWaypoint(writer, filon);
                    }
                    
                    writer.WriteEndElement(); // gpx
                    writer.WriteEndDocument();
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Exporte un itinéraire au format GPX (route + waypoints)
        /// </summary>
        public bool ExportRouteToGpx(RouteInfo route, string filePath)
        {
            try
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    Encoding = Encoding.UTF8
                };
                
                using (var writer = XmlWriter.Create(filePath, settings))
                {
                    writer.WriteStartDocument();
                    
                    // Root GPX element
                    writer.WriteStartElement("gpx", GPX_NAMESPACE);
                    writer.WriteAttributeString("version", GPX_VERSION);
                    writer.WriteAttributeString("creator", CREATOR);
                    
                    // Metadata
                    writer.WriteStartElement("metadata");
                    writer.WriteElementString("name", $"Itinéraire : {route.StartName} ? {route.EndName}");
                    writer.WriteElementString("desc", $"Distance: {route.FormattedDistance}, Durée: {route.FormattedDuration}");
                    writer.WriteElementString("time", DateTime.UtcNow.ToString("o"));
                    writer.WriteEndElement(); // metadata
                    
                    // Waypoint départ
                    writer.WriteStartElement("wpt");
                    writer.WriteAttributeString("lat", route.Start.Lat.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("lon", route.Start.Lng.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteElementString("name", "déPART");
                    writer.WriteElementString("desc", route.StartName);
                    writer.WriteElementString("sym", "Flag, Green");
                    writer.WriteEndElement(); // wpt
                    
                    // Waypoint arrivée
                    writer.WriteStartElement("wpt");
                    writer.WriteAttributeString("lat", route.End.Lat.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("lon", route.End.Lng.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
                    writer.WriteElementString("name", "ARRIVéE");
                    writer.WriteElementString("desc", route.EndName);
                    writer.WriteElementString("sym", "Flag, Red");
                    writer.WriteEndElement(); // wpt
                    
                    // Route (tracé complet)
                    writer.WriteStartElement("rte");
                    writer.WriteElementString("name", $"{route.StartName} ? {route.EndName}");
                    writer.WriteElementString("desc", $"Transport: {GetTransportName(route.TransportType)}");
                    
                    // Points de route
                    foreach (var point in route.Points)
                    {
                        writer.WriteStartElement("rtept");
                        writer.WriteAttributeString("lat", point.Lat.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
                        writer.WriteAttributeString("lon", point.Lng.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
                        writer.WriteEndElement(); // rtept
                    }
                    
                    writer.WriteEndElement(); // rte
                    
                    // Track (pour compatibilité avec certains GPS)
                    writer.WriteStartElement("trk");
                    writer.WriteElementString("name", $"Itinéraire {route.FormattedDistance}");
                    
                    writer.WriteStartElement("trkseg");
                    foreach (var point in route.Points)
                    {
                        writer.WriteStartElement("trkpt");
                        writer.WriteAttributeString("lat", point.Lat.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
                        writer.WriteAttributeString("lon", point.Lng.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
                        writer.WriteEndElement(); // trkpt
                    }
                    writer.WriteEndElement(); // trkseg
                    writer.WriteEndElement(); // trk
                    
                    writer.WriteEndElement(); // gpx
                    writer.WriteEndDocument();
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Exporte filons + itinéraire combiné au format GPX
        /// </summary>
        public bool ExportCompleteToGpx(List<Filon> filons, RouteInfo? route, string filePath)
        {
            try
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    Encoding = Encoding.UTF8
                };
                
                using (var writer = XmlWriter.Create(filePath, settings))
                {
                    writer.WriteStartDocument();
                    
                    // Root GPX element
                    writer.WriteStartElement("gpx", GPX_NAMESPACE);
                    writer.WriteAttributeString("version", GPX_VERSION);
                    writer.WriteAttributeString("creator", CREATOR);
                    
                    // Metadata
                    writer.WriteStartElement("metadata");
                    writer.WriteElementString("name", "Export Complet WMine");
                    writer.WriteElementString("desc", $"{filons.Count} filon(s) + itinéraire");
                    writer.WriteElementString("time", DateTime.UtcNow.ToString("o"));
                    writer.WriteEndElement(); // metadata
                    
                    // Waypoints (filons)
                    var validFilons = filons.Where(f => 
                        f.Latitude.HasValue && 
                        f.Longitude.HasValue).ToList();
                    
                    foreach (var filon in validFilons)
                    {
                        WriteWaypoint(writer, filon);
                    }
                    
                    // Route si présente
                    if (route != null && route.Points.Count > 0)
                    {
                        writer.WriteStartElement("rte");
                        writer.WriteElementString("name", $"{route.StartName} ? {route.EndName}");
                        
                        foreach (var point in route.Points)
                        {
                            writer.WriteStartElement("rtept");
                            writer.WriteAttributeString("lat", point.Lat.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
                            writer.WriteAttributeString("lon", point.Lng.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
                            writer.WriteEndElement();
                        }
                        
                        writer.WriteEndElement(); // rte
                    }
                    
                    writer.WriteEndElement(); // gpx
                    writer.WriteEndDocument();
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private void WriteWaypoint(XmlWriter writer, Filon filon)
        {
            if (!filon.Latitude.HasValue || !filon.Longitude.HasValue)
                return;
            
            writer.WriteStartElement("wpt");
            writer.WriteAttributeString("lat", filon.Latitude.Value.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
            writer.WriteAttributeString("lon", filon.Longitude.Value.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
            
            // Nom (limité é 10 caractéres pour compatibilité Garmin)
            var shortName = filon.Nom.Length > 10 ? filon.Nom.Substring(0, 10) : filon.Nom;
            writer.WriteElementString("name", shortName);
            
            // Description compléte
            var description = new StringBuilder();
            description.AppendLine($"Filon: {filon.Nom}");
            description.AppendLine($"Minéral: {MineralColors.GetDisplayName(filon.MatierePrincipale)}");
            
            if (filon.MatieresSecondaires.Any())
            {
                description.AppendLine($"Secondaires: {string.Join(", ", filon.MatieresSecondaires.Take(3).Select(m => MineralColors.GetDisplayName(m)))}");
            }
            
            if (!string.IsNullOrWhiteSpace(filon.Notes))
            {
                var notesShort = filon.Notes.Length > 200 ? filon.Notes.Substring(0, 200) + "..." : filon.Notes;
                description.AppendLine($"Notes: {notesShort}");
            }
            
            writer.WriteElementString("desc", description.ToString().Trim());
            
            // Symbole selon le minéral
            writer.WriteElementString("sym", GetSymbolForMineral(filon.MatierePrincipale));
            
            // Type
            writer.WriteElementString("type", "Mine/Filon");
            
            writer.WriteEndElement(); // wpt
        }
        
        private string GetSymbolForMineral(MineralType mineral)
        {
            return mineral switch
            {
                MineralType.Cuivre => "Diamond, Red",
                MineralType.Fer => "Diamond, Black",
                MineralType.Plomb => "Diamond, Gray",
                MineralType.Zinc => "Diamond, Blue",
                MineralType.Argent => "Diamond, White",
                MineralType.Grenats => "Diamond, Red",
                MineralType.Tourmaline => "Diamond, Green",
                MineralType.Améthyste => "Diamond, Purple",
                MineralType.Estérellite => "Diamond, Yellow",
                MineralType.Uranifères => "Diamond, Yellow",
                MineralType.Baryum => "Diamond, White",
                MineralType.Fluor => "Diamond, Green",
                MineralType.Antimoine => "Diamond, Gray",
                MineralType.Andalousite => "Diamond, Pink",
                MineralType.Disthéne => "Diamond, Blue",
                MineralType.Staurotite => "Diamond, Brown",
                _ => "Pin, Blue"
            };
        }
        
        private string GetTransportName(TransportType type)
        {
            return type switch
            {
                TransportType.Car => "Voiture",
                TransportType.Walking => "é pied",
                TransportType.Cycling => "Vélo",
                _ => "Inconnu"
            };
        }
        
        /// <summary>
        /// Valide un fichier GPX
        /// </summary>
        public (bool IsValid, string ErrorMessage) ValidateGpx(string filePath)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(filePath);
                
                // Vérifier namespace GPX
                if (doc.DocumentElement?.NamespaceURI != GPX_NAMESPACE)
                {
                    return (false, "Namespace GPX invalide");
                }
                
                // Vérifier version
                var version = doc.DocumentElement?.GetAttribute("version");
                if (string.IsNullOrEmpty(version))
                {
                    return (false, "Version GPX manquante");
                }
                
                return (true, "Fichier GPX valide");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur: {ex.Message}");
            }
        }
    }
}
