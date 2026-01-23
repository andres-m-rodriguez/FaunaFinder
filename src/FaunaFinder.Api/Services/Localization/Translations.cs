namespace FaunaFinder.Api.Services.Localization;

public static class Translations
{
    public static IReadOnlyDictionary<string, string> English { get; } = new Dictionary<string, string>
    {
        // Navigation
        ["Nav_Map"] = "Map",
        ["Nav_Species"] = "Species",
        ["Nav_Pueblos"] = "Municipalities",
        ["Nav_About"] = "About",

        // Common
        ["AppTitle"] = "FaunaFinder",
        ["Loading"] = "Loading...",
        ["Back"] = "Back",
        ["TryAgain"] = "Try Again",
        ["ViewDetails"] = "View Details",
        ["ViewAllSpecies"] = "View All Species",
        ["ViewAll"] = "View All",
        ["AllSpecies"] = "All Species",
        ["Details"] = "Details",
        ["ViewOnMap"] = "View on Map",

        // Errors
        ["Error_SomethingWentWrong"] = "Something went wrong",
        ["Error_UnexpectedError"] = "An unexpected error occurred. Please try again.",
        ["Error_SpeciesNotFound"] = "Species not found.",
        ["Error_MunicipalityNotFound"] = "Municipality not found.",

        // Home Page
        ["Home_ClickMunicipality"] = "Click on a municipality on the map to view species and conservation information.",
        ["Home_NoSpeciesData"] = "No species data available for this municipality.",
        ["Home_SpeciesFound"] = "{0} species found",
        ["Home_SpeciesInDatabase"] = "{0} species in database",
        ["Home_ConservationLinks"] = "Conservation Links",
        ["Home_ClearLocations"] = "Clear",
        ["Home_LocationsFound"] = "{0} location(s) found",
        ["Home_ViewAllLocations"] = "View All Locations",
        ["Home_BackToSpecies"] = "Back to Species Details",
        ["Home_UnnamedLocation"] = "Location",

        // Species Page
        ["Species_Title"] = "Species",
        ["Species_Description"] = "Explore the species of Puerto Rico and discover where they can be found.",
        ["Species_SearchPlaceholder"] = "Search species...",
        ["Species_NoResults"] = "No species found matching your search.",
        ["Species_Municipality"] = "municipality",
        ["Species_Municipalities"] = "municipalities",
        ["Species_Showing"] = "Showing {0}-{1} of {2} species",

        // Species Detail Page
        ["SpeciesDetail_FoundIn"] = "Found in {0} {1}",
        ["SpeciesDetail_MunicipalitiesTitle"] = "Municipalities",
        ["SpeciesDetail_NoMunicipalityData"] = "No municipality data available for this species.",
        ["SpeciesDetail_ConservationLinksTitle"] = "Conservation Links",
        ["SpeciesDetail_NoConservationLinks"] = "No conservation links available for this species.",
        ["SpeciesDetail_ViewLocations"] = "View Locations",

        // Pueblos Page
        ["Pueblos_Title"] = "Municipalities of Puerto Rico",
        ["Pueblos_Description"] = "Explore the municipalities of Puerto Rico and discover their biodiversity.",
        ["Pueblos_SearchPlaceholder"] = "Search municipalities...",
        ["Pueblos_NoResults"] = "No municipalities found matching your search.",
        ["Pueblos_Species"] = "species",
        ["Pueblos_Showing"] = "Showing {0}-{1} of {2} municipalities",

        // Pueblo Detail Page
        ["PuebloDetail_SpeciesInMunicipality"] = "Species in this Municipality",
        ["PuebloDetail_NoSpeciesData"] = "No species data available for this municipality.",
        ["PuebloDetail_NoConservationLinks"] = "No conservation links available for this species.",

        // About Page
        ["About_Title"] = "About FaunaFinder",
        ["About_WhatIsTitle"] = "What is FaunaFinder?",
        ["About_WhatIsDescription"] = "FaunaFinder is an interactive web application that helps users explore conservation information for Puerto Rico's municipalities. Click on any municipality on the map to discover the species that inhabit that region, along with relevant NRCS conservation practices and FWS action recommendations.",
        ["About_DataSourcesTitle"] = "Data Sources",
        ["About_NrcsPractices"] = "NRCS Practices:",
        ["About_NrcsPracticesDesc"] = "Natural Resources Conservation Service conservation practice standards",
        ["About_FwsActions"] = "FWS Actions:",
        ["About_FwsActionsDesc"] = "U.S. Fish and Wildlife Service recommended conservation actions",
        ["About_SpeciesData"] = "Species Data:",
        ["About_SpeciesDataDesc"] = "Species occurrence and habitat information for Puerto Rico",

        // Filter and Sort
        ["Filter_Sort"] = "Sort",
        ["Filter_NameAZ"] = "Name (A-Z)",
        ["Filter_NameZA"] = "Name (Z-A)",
        ["Filter_ScientificAZ"] = "Scientific (A-Z)",
        ["Filter_ScientificZA"] = "Scientific (Z-A)",
        ["Filter_Filters"] = "Filters",
        ["Filter_ClearAll"] = "Clear All",
        ["Filter_Apply"] = "Apply",
        ["Filter_NrcsPractices"] = "NRCS Practices",
        ["Filter_FwsActions"] = "FWS Actions",
        ["Filter_ShowingFiltered"] = "Showing {0} of {1} species",
        ["Filter_NoMatches"] = "No species match the selected filters.",

        // Page Titles
        ["PageTitle_Home"] = "FaunaFinder - Puerto Rico",
        ["PageTitle_Species"] = "Species - FaunaFinder",
        ["PageTitle_Pueblos"] = "Municipalities - FaunaFinder",
        ["PageTitle_About"] = "About - FaunaFinder",

        // Species Near Me
        ["NearMe_Title"] = "Species Near Me",
        ["NearMe_Button"] = "Near Me",
        ["NearMe_SelectRadius"] = "Select search radius",
        ["NearMe_Searching"] = "Searching for species...",
        ["NearMe_NoSpeciesFound"] = "No species found within this radius. Try expanding the search area.",
        ["NearMe_SpeciesFound"] = "{0} species found within {1}km",
        ["NearMe_UseLocateFirst"] = "Use the locate button on the map to enable species search near your location.",
        ["NearMe_ShowLocations"] = "Show Locations",
        ["NearMe_HideLocations"] = "Hide Locations",

        // Export
        ["Export_Button"] = "Export",
        ["Export_PDF"] = "Download PDF",
        ["Export_CSV"] = "Download CSV",
        ["Export_Generating"] = "Generating report...",
    };

    public static IReadOnlyDictionary<string, string> Spanish { get; } = new Dictionary<string, string>
    {
        // Navigation
        ["Nav_Map"] = "Mapa",
        ["Nav_Species"] = "Especies",
        ["Nav_Pueblos"] = "Pueblos",
        ["Nav_About"] = "Acerca de",

        // Common
        ["AppTitle"] = "FaunaFinder",
        ["Loading"] = "Cargando...",
        ["Back"] = "Volver",
        ["TryAgain"] = "Intentar de nuevo",
        ["ViewDetails"] = "Ver detalles",
        ["ViewAllSpecies"] = "Ver todas las especies",
        ["ViewAll"] = "Ver todo",
        ["AllSpecies"] = "Todas las especies",
        ["Details"] = "Detalles",
        ["ViewOnMap"] = "Ver en mapa",

        // Errors
        ["Error_SomethingWentWrong"] = "Algo salió mal",
        ["Error_UnexpectedError"] = "Ocurrió un error inesperado. Por favor, inténtelo de nuevo.",
        ["Error_SpeciesNotFound"] = "Especie no encontrada.",
        ["Error_MunicipalityNotFound"] = "Municipio no encontrado.",

        // Home Page
        ["Home_ClickMunicipality"] = "Haz clic en un municipio en el mapa para ver información sobre especies y conservación.",
        ["Home_NoSpeciesData"] = "No hay datos de especies disponibles para este municipio.",
        ["Home_SpeciesFound"] = "{0} especies encontradas",
        ["Home_SpeciesInDatabase"] = "{0} especies en la base de datos",
        ["Home_ConservationLinks"] = "Enlaces de conservación",
        ["Home_ClearLocations"] = "Cerrar",
        ["Home_LocationsFound"] = "{0} ubicación(es) encontrada(s)",
        ["Home_ViewAllLocations"] = "Ver todas las ubicaciones",
        ["Home_BackToSpecies"] = "Volver a detalles de especie",
        ["Home_UnnamedLocation"] = "Ubicación",

        // Species Page
        ["Species_Title"] = "Especies",
        ["Species_Description"] = "Explora las especies de Puerto Rico y descubre dónde se pueden encontrar.",
        ["Species_SearchPlaceholder"] = "Buscar especies...",
        ["Species_NoResults"] = "No se encontraron especies que coincidan con tu búsqueda.",
        ["Species_Municipality"] = "municipio",
        ["Species_Municipalities"] = "municipios",
        ["Species_Showing"] = "Mostrando {0}-{1} de {2} especies",

        // Species Detail Page
        ["SpeciesDetail_FoundIn"] = "Encontrada en {0} {1}",
        ["SpeciesDetail_MunicipalitiesTitle"] = "Municipios",
        ["SpeciesDetail_NoMunicipalityData"] = "No hay datos de municipios disponibles para esta especie.",
        ["SpeciesDetail_ConservationLinksTitle"] = "Enlaces de conservación",
        ["SpeciesDetail_NoConservationLinks"] = "No hay enlaces de conservación disponibles para esta especie.",
        ["SpeciesDetail_ViewLocations"] = "Ver ubicaciones",

        // Pueblos Page
        ["Pueblos_Title"] = "Pueblos de Puerto Rico",
        ["Pueblos_Description"] = "Explora los municipios de Puerto Rico y descubre su biodiversidad.",
        ["Pueblos_SearchPlaceholder"] = "Buscar municipios...",
        ["Pueblos_NoResults"] = "No se encontraron municipios que coincidan con tu búsqueda.",
        ["Pueblos_Species"] = "especies",
        ["Pueblos_Showing"] = "Mostrando {0}-{1} de {2} municipios",

        // Pueblo Detail Page
        ["PuebloDetail_SpeciesInMunicipality"] = "Especies en este municipio",
        ["PuebloDetail_NoSpeciesData"] = "No hay datos de especies disponibles para este municipio.",
        ["PuebloDetail_NoConservationLinks"] = "No hay enlaces de conservación disponibles para esta especie.",

        // About Page
        ["About_Title"] = "Acerca de FaunaFinder",
        ["About_WhatIsTitle"] = "¿Qué es FaunaFinder?",
        ["About_WhatIsDescription"] = "FaunaFinder es una aplicación web interactiva que ayuda a los usuarios a explorar información de conservación para los municipios de Puerto Rico. Haz clic en cualquier municipio en el mapa para descubrir las especies que habitan en esa región, junto con las prácticas de conservación del NRCS y las recomendaciones de acción del FWS.",
        ["About_DataSourcesTitle"] = "Fuentes de datos",
        ["About_NrcsPractices"] = "Prácticas NRCS:",
        ["About_NrcsPracticesDesc"] = "Estándares de prácticas de conservación del Servicio de Conservación de Recursos Naturales",
        ["About_FwsActions"] = "Acciones FWS:",
        ["About_FwsActionsDesc"] = "Acciones de conservación recomendadas por el Servicio de Pesca y Vida Silvestre de EE.UU.",
        ["About_SpeciesData"] = "Datos de especies:",
        ["About_SpeciesDataDesc"] = "Información sobre ocurrencia y hábitat de especies en Puerto Rico",

        // Filter and Sort
        ["Filter_Sort"] = "Ordenar",
        ["Filter_NameAZ"] = "Nombre (A-Z)",
        ["Filter_NameZA"] = "Nombre (Z-A)",
        ["Filter_ScientificAZ"] = "Científico (A-Z)",
        ["Filter_ScientificZA"] = "Científico (Z-A)",
        ["Filter_Filters"] = "Filtros",
        ["Filter_ClearAll"] = "Limpiar",
        ["Filter_Apply"] = "Aplicar",
        ["Filter_NrcsPractices"] = "Prácticas NRCS",
        ["Filter_FwsActions"] = "Acciones FWS",
        ["Filter_ShowingFiltered"] = "Mostrando {0} de {1} especies",
        ["Filter_NoMatches"] = "Ninguna especie coincide con los filtros seleccionados.",

        // Page Titles
        ["PageTitle_Home"] = "FaunaFinder - Puerto Rico",
        ["PageTitle_Species"] = "Especies - FaunaFinder",
        ["PageTitle_Pueblos"] = "Pueblos - FaunaFinder",
        ["PageTitle_About"] = "Acerca de - FaunaFinder",

        // Species Near Me
        ["NearMe_Title"] = "Especies cercanas",
        ["NearMe_Button"] = "Cercanas",
        ["NearMe_SelectRadius"] = "Seleccionar radio de busqueda",
        ["NearMe_Searching"] = "Buscando especies...",
        ["NearMe_NoSpeciesFound"] = "No se encontraron especies dentro de este radio. Intenta ampliar el area de busqueda.",
        ["NearMe_SpeciesFound"] = "{0} especies encontradas dentro de {1}km",
        ["NearMe_UseLocateFirst"] = "Usa el boton de ubicacion en el mapa para habilitar la busqueda de especies cerca de tu ubicacion.",
        ["NearMe_ShowLocations"] = "Mostrar Ubicaciones",
        ["NearMe_HideLocations"] = "Ocultar Ubicaciones",

        // Export
        ["Export_Button"] = "Exportar",
        ["Export_PDF"] = "Descargar PDF",
        ["Export_CSV"] = "Descargar CSV",
        ["Export_Generating"] = "Generando informe...",
    };
}
