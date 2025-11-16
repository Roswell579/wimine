# PR: test Pr

Refactor: inject `IMineralRepository` and `PhotoService`, add `PhotoGalleryPanel`, integrate DI and photo gallery.

Changes included:
- Inject `IMineralRepository` and `PhotoService` into `Form1` and panels
- Created `Forms/PhotoGalleryPanel.cs` and wired opening from `MineralsPanel`
- Replaced static `MineralDataService` usages by `IMineralRepository` where applicable
- Registered services in `Program.cs` and passed into `Form1` constructor

Branch: `refactor/mineraldata`

Build: Successful (local)

Please open a pull request on GitHub with title: `test Pr` and this description.
