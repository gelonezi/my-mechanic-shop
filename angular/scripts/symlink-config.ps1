# Symlink Configuration
# Shared configuration for symlink management scripts

# Define the package directories that need symlink management
# example: "../../modules/Volo.Abp.Identity.Pro/angular"
#
# DELIBERATELY EMPTY. Do not register modules/<Name>/angular here.
#
# These scripts symlink this app's @angular/@abp/etc into a module workspace so
# both compile against one instance. That matters when a host compiles library
# SOURCE. It does not apply here: the Catalog library is built with ng-packagr
# to modules/<Name>/angular/dist/<name> and the host consumes that package via
# tsconfig "paths", so ng-packagr emits partial-compiled output and only the
# HOST's Angular is ever bundled. The module's own @angular is used solely to
# build the library.
#
# Registering a module here actively breaks it: Node resolves the junction to
# its real path, so @angular/build looks for ng-packagr in THIS app's
# node_modules, where an application has no reason to have it, and `ng build
# <lib>` dies with a silent exit 1. It also couples the module's build to this
# workspace, which defeats building the module's frontend independently.
#
# Note also that symlinks:remove deletes the module's ENTIRE node_modules, and
# setup deletes each original package before junctioning it. Neither is
# reversible without a reinstall.
$script:PackageDirectories = @()

# Define packages that should be symlinked
$script:PackagesToSymlink = @(
    "@angular",
    "@abp",
    "@volo",
    "@volosoft",
    "@swimlane",
    "@ngx-validate",
    "@ng-bootstrap",
    "rxjs",
    "cropperjs",
    "angularx-qrcode",
    "qrcode",
    "tslib",
    "@uppy",
    "@microsoft",
    "ng-zorro-antd"
)

# Helper function to get package directories
function Get-PackageDirectories {
    return $script:PackageDirectories
}

# Helper function to get packages to symlink
function Get-PackagesToSymlink {
    return $script:PackagesToSymlink
}

# Functions are available when script is dot-sourced