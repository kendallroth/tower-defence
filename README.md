# Tower Defence

> Simple Unity tower defence game (hexagonal grid)

## Development

_TODO_

### Assets

There are several assets that must be installed to work with the project!

- `External/`
  - ALINE 
  - Medieval Builder Kit (Kenney)
  - Prefab Painter (nTools)
  - Message Dispatched (ooti)
  - Serialized Dictionary Light (Rotary Heart)
  - Tower Defence Kit (Kenney)
- `Plugins/`
  - Odin Inspector (Sirenix)

## Caveats

### Unity Serialization

Unity does not serialize public properties or private attributes by default, which can lead to some interesting/unexpected scenarios. Public attributes will be properly serialized and displayed in the editor.

Public properties can be displayed in the editor **but** will not be serialized. Instead, use a serialized private property along with a public accessor to both display and serialize the data.

Private fields can be serialized and displayed in the editor with the `[SerializeField]` attribute. However, without this attribute private fields will not be serialized (stored)! If wanting to serialize a private field **but** not display it, add both the `[SerializeField]` and `[HideInInspector]` attributes.

