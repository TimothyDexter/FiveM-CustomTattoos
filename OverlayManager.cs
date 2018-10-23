/*
 * 
 * Overlay Manager
 * Author: Timothy Dexter
 * Release: 0.0.2
 * Date: 2/10/18
 * 
 * 
 * Known Issues
 * 
 * Please send any edits/improvements/bugs to this script back to the author. 
 * 
 * Usage 
 * 
 * History:
 * Revision 0.0.1 2018/02/03 07:21:15 EDT TimothyDexter 
 * - Initial release
 * Revision 0.0.2 2018/02/10 22:33:01 EDT TimothyDexter 
 * - Add method to check for compatible models
 *
 */

using System;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace Roleplay.Client.Classes.Player
{
	internal class OverlayManager
	{
		public static List<OverlayModel> CurrentOverlays = new List<OverlayModel>();

		/// <summary>
		///     Refresh current overlays
		/// </summary>
		public static void RefreshCurrentOverlays() {
			try {
				API.ClearPedDecorations( Game.PlayerPed.Handle );

				foreach( var overlayModel in CurrentOverlays ) {
					var overlay = OverlaysTable.GetOverlayModel( overlayModel.Id );

					if(overlay == null ) { Log.Verbose($"Overlay is null");
						return;
					}

					for( var i = 0; i < overlayModel.ApplyCount; i++ ) {
						var collection = string.Concat( overlay.Collection, "_overlays" );
						API.ApplyPedOverlay( Game.PlayerPed.Handle,
							(uint)API.GetHashKey( collection ),
							(uint)API.GetHashKey( overlay.HashName ) );
					}
				}
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}

		/// <summary>
		///     Add new overlay to player collection or increment application count
		/// </summary>
		public static void AddOverlayToPlayer( int id ) {
			try {
				if( IsInCurrentOverlays( id ) ) {
					Log.Verbose(
						$"Cannot add new overlay: Player collection already contains id={id}.  Incrementing applicationCount" );
					UpdateOverlayApplyCount( id );
					return;
				}

				CurrentOverlays.Add( new OverlayModel( id ) );
				SaveOverlays();
				RefreshCurrentOverlays();
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}

		/// <summary>
		///     Load player overlays
		/// </summary>
		public static void LoadOverlays() {
			try {
				var savedOverlays = CurrentPlayer.CharacterData.PedData.Overlays;
				if( savedOverlays == null ) {
					Log.Verbose( $"savedOverlays is null" );
					return;
				}
				Log.Verbose($"Loading overlays");

				foreach( var overlay in savedOverlays ) CurrentOverlays.Add( new OverlayModel( overlay.Key, overlay.Value ) );
				RefreshCurrentOverlays();
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}

		/// <summary>
		///     Save player overlays
		/// </summary>
		public static void SaveOverlays() {
			try {
				var overlaysToSave = new Dictionary<ushort, byte>();
				foreach( var overlay in CurrentOverlays ) {
					var key = (ushort)overlay.Id;
					var value = (byte)overlay.ApplyCount;
					overlaysToSave[key] = value;
				}

				var saveData = CurrentPlayer.CharacterData.PedData;
				saveData.Overlays = overlaysToSave;
				CurrentPlayer.CharacterData.PedData.Overlays = overlaysToSave;
				BaseScript.TriggerServerEvent( "Session.SavePedData", JsonConvert.SerializeObject( saveData ) );
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}

		/// <summary>
		///     Remove tattoos from a body part zone
		/// </summary>
		/// <param name="zone">body part zone</param>
		public static void RemoveTattoosFromZone( OverlaysTable.OverlayZones zone ) {
			try {
				var overlaysToRemove = new List<int>();
				foreach( var overlayModel in CurrentOverlays ) {
					if( !OverlaysTable.OverlayTattooTable.TryGetValue( overlayModel.Id, out _ ) ) continue;

					var overlay = OverlaysTable.GetOverlayModel( overlayModel.Id );
					if( overlay.OverlayZone == zone ) overlaysToRemove.Add( overlayModel.Id );
				}

				foreach( var overlayId in overlaysToRemove ) RemoveOverlayFromPlayer( overlayId, false );

				SaveOverlays();
				RefreshCurrentOverlays();
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}

		/// <summary>
		///     Remove all tattoos from the player
		/// </summary>
		public static void RemoveTattoos() {
			try {
				RemoveOverlaysInTable( OverlaysTable.OverlayTattooTable );
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}

		/// <summary>
		///     Remove all badges from the player
		/// </summary>
		public static void RemoveBadges() {
			try {
				RemoveOverlaysInTable( OverlaysTable.OverlayBadgeTable );
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}

		/// <summary>
		///     Remove all hairstyles from the player
		/// </summary>
		public static void RemoveHairstyles() {
			try {
				RemoveOverlaysInTable( OverlaysTable.OverlayHairTable );
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}

		/// <summary>
		///     Remove all overlays from player contained in table
		/// </summary>
		/// <param name="table">table to remove overlays from</param>
		private static void RemoveOverlaysInTable( IReadOnlyDictionary<int, OverlaysTable.OverlayModel> table ) {
			try {
				var overlaysToRemove = new List<int>();
				foreach( var overlayModel in CurrentOverlays ) {
					if( !table.TryGetValue( overlayModel.Id, out _ ) ) continue;
					overlaysToRemove.Add( overlayModel.Id );
				}

				foreach( var overlayId in overlaysToRemove ) RemoveOverlayFromPlayer( overlayId, false );

				RefreshCurrentOverlays();
				SaveOverlays();
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}

		/// <summary>
		///     Remove individual overlay from player
		/// </summary>
		/// <param name="id">overlay id</param>
		/// <param name="instantUpdate">update player collection immediately</param>
		public static void RemoveOverlayFromPlayer( int id, bool instantUpdate = true ) {
			try {
				var index = GetIndexOfOverlay( id );
				if( index < 0 ) return;

				CurrentOverlays.RemoveAt( index );
				if( !instantUpdate ) return;

				SaveOverlays();
				RefreshCurrentOverlays();
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}

		/// <summary>
		///     Update overlay application count
		/// </summary>
		public static void UpdateOverlayApplyCount( int id ) {
			try {
				if( !IsInCurrentOverlays( id ) ) return;

				var index = GetIndexOfOverlay( id );
				if( !IsIndexInRange( index ) ) {
					Log.Verbose( $"Cannot update applyCount: Player collection does not contain id={id}" );
					return;
				}

				if( CurrentOverlays[index].ApplyCount < 9 )
					CurrentOverlays[index].ApplyCount = CurrentOverlays[index].ApplyCount + 1;
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}

		/// <summary>
		///     Is the overlay id in current overlays
		/// </summary>
		/// <param name="id"></param>
		public static bool IsInCurrentOverlays( int id ) {
			try {
				return GetIndexOfOverlay( id ) >= 0;
			}
			catch( Exception ex ) {
				Log.Error( ex );
				return false;
			}
		}

		/// <summary>
		///     Is overlay index in range
		/// </summary>
		/// <param name="index"></param>
		private static bool IsIndexInRange( int index ) {
			try {
				return index >= 0 && index < CurrentOverlays.Count;
			}
			catch( Exception ex ) {
				Log.Error( ex );
				return false;
			}
		}

		/// <summary>
		///     Is the overlay id in current overlays
		/// </summary>
		/// <param name="id"></param>
		private static int GetIndexOfOverlay( int id ) {
			try {
				for( var index = 0; index < CurrentOverlays.Count; index++ )
					if( CurrentOverlays[index].Id == id )
						return index;
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}

			return -1;
		}

		/// <summary>
		///     Get new sub menu model
		/// </summary>
		/// <param name="menuName"></param>
		public static MenuItemSubMenu GetSubMenu( string menuName ) {
			try {
				return new MenuItemSubMenu {
					Title = menuName,
					SubMenu = new MenuModel {
						headerTitle = menuName,
						statusTitle = "",
						numVisibleItems = 10,
						menuItems = new List<MenuItem>()
					}
				};
			}
			catch( Exception ex ) {
				Log.Error( ex );
				return null;
			}
		}

		/// <summary>
		///     Get dictionary of submenus for each collection
		/// </summary>
		public static Dictionary<string, MenuItemSubMenu> GetCollectionZones() {
			try {
				return new Dictionary<string, MenuItemSubMenu> {
					{"mpbeach", GetSubMenu( "Beach" )},
					{"mpbiker", GetSubMenu( "Biker" )},
					{"mpbusiness", GetSubMenu( "Business" )},
					{"mpchristmas2", GetSubMenu( "Christmas" )},
					{"mpexecutive", GetSubMenu( "Executive" )},
					{"mpgunrunning", GetSubMenu( "Gun Running" )},
					{"mphalloween", GetSubMenu( "Halloween" )},
					{"mpheist", GetSubMenu( "Heist" )},
					{"mphipster", GetSubMenu( "Hipster" )},
					{"mpimportexport", GetSubMenu( "Import & Export" )},
					{"mpindependence", GetSubMenu( "Independence" )},
					{"mplowrider", GetSubMenu( "Lowrider" )},
					{"mplowrider2", GetSubMenu( "Lowrider II" )},
					{"mpluxe", GetSubMenu( "Deluxe" )},
					{"mpluxe2", GetSubMenu( "Deluxe II" )},
					{"multiplayer", GetSubMenu( "Multi" )},
					{"singleplayer", GetSubMenu( "Single" )},
					{"mpstunt", GetSubMenu( "Stunt" )},
					{"mpvalentines", GetSubMenu( "Valentines" )},
					{"blazingtattoo", GetSubMenu("Blazing Tattoo") },
				};
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}

			return null;
		}

		public class OverlayModel
		{
			public OverlayModel( int id, int applyCount = 1 ) {
				Id = id;
				ApplyCount = applyCount;
			}

			public int Id { get; }
			public int ApplyCount { get; set; }
		}
	}
}
