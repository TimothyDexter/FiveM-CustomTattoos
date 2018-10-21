# FiveM-CustomTattoos

1.  Re-size the .png image to be used so that the dimensions are powers of 2 (e.g. 512x512, 256x1024, etc.)
Note: This isn't a requirement but it will take advantage of mipmaps which improve image quality when applied to the characters.
2.  Rename the image to the overlay name you'll use, e.g. blazingtattoo_60.png
Note: Anything proceeding the underscore must be the same as the tattoo collection name.
3.  Export a tattoo from OpenIV (e.g. mp_bus_tat_m_006.ytd)
4.  Rename tattoo filename to overlay name mp_bus_tat_m_006.ytd -> blazingtattoo_60.ytd
Note: Anything proceeding the underscore must be the same as the tattoo collection name.
5.  Open the tattoo .ytd in OpenIV texture editor and use the rename option to match the image filename, e.g. blazingtattoo_60
6.  While still in texture editor, use the replace option and select the image file with the same name (e.g. blazingtattoo_60.png).
7.  You may possible have 2 textures in the tattoo at this point, delete the older one if it still exists and save the .ytd
8.  Move the .ytd to resources\[familyrp]\new_overlays\stream
9.  Open/Create an .xml similar to blazingtattoos_overalys.xml located in \resources\[familyrp]\new_overlays
This file dictates the tattoo gender, placement, and scale.  Male tattoos will only work on males and vice versa.  If you hear of someone saying the tattoo shows for them but not others, it is because they are using a tattoo opposite of their gender.  Using GENDER_DONTCARE should make it so that tattoos show up for regardless of gender.

Follow previous examples located in blazingtattoos_overlays.xml
Note: Anything proceeding _overlays must be the same as the tattoo collection name.
The key components you need to edit:
```xml
    <Item>
-->      <uvPos x="0.720000" y="0.670000" />
-->      <scale x="0.280000" y="0.350000" />
      <rotation value="0.000000" />
-->      <nameHash>blazingtattoo_05_A</nameHash>
-->      <txdHash>blazingtattoo_05</txdHash>
-->      <txtHash>blazingtattoo_05</txtHash>
-->      <zone>ZONE_RIGHT_ARM</zone>
-->      <type>TYPE_TATTOO</type>
      <faction>FM</faction>
      <garment>All</garment>
-->      <gender>GENDER_DONTCARE</gender>
      <award />
      <awardLevel />
    </Item>
```
If it is a fe(male) tattoo, it should be named blazingtattoo_05_F or _M instead.
If it is a badge or hairstyle, you need to change <type> property to reflect that.

Potential type's are:
TYPE_BADGE
TYPE_TATTOO

Potential zone's are:
ZONE_HEAD
ZONE_LEFT_ARM
ZONE_LEFT_LEG
ZONE_RIGHT_ARM
ZONE_RIGHT_LEG
ZONE_TORSO

10.  Open/Create shoptattoos.meta file and add an entry there for the overlay. These don't require much work.

The key components you need to edit:
```xml
	<Item>
	    <id value="92610"/>
	    <cost value="50"/>
-->	    <textLabel>BLAZINGTATTOO_TAT_004</textLabel>
-->	    <collection>blazingtattoo_overlays</collection>
-->	    <preset>blazingtattoo_04_A</preset>
	    <zone>PDZ_HEAD</zone>
	    <eFacing>TATTOO_RIGHT</eFacing>
	    <updateGroup>ARM_RIGHT_LOWER_SLEEVE</updateGroup>
	    <eFaction>TATTOO_MP_FM</eFaction>
	    <lockHash>CUSTOM_TATTOOS</lockHash>
	</Item>
```
Collection needs to be the same name as your XML file.
None of the other information matters as far as I've been able to tell in tattoos.meta.

I find it easier to just load the tattoos in the game and edit their position/scale while live.  You can use icecon or the console window to issue "restart new_overlays" and the tattoo will update.
