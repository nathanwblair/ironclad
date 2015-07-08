Ironclad aims, in the long term, to allow IronPython users to transparently import and use any compiled CPython extensions. Ironclad works with IronPython 2.6 and targets CPython 2.6 on 32-bit Windows; efforts to support other platforms are underway.

## Quick Start ##

  * Download the latest binary package
  * Unzip it into `C:`
  * `cd` to `C:\ironclad-v2.6.0rc1-bin`
  * run `ipy`
  * `import ironclad`

If the above steps pass without error, you are ready to import CPython extensions. Assuming you have a CPython 2.6 install located at `C:\Python26`, add the following locations to your `sys.path`:

  * `C:\Python26\Dlls`
  * `C:\Python26\Lib`
  * `C:\Python26\Lib\site-packages`

...and you should be able to import and use many of the extensions in your CPython install.

  * `import bz2`
  * `import numpy`

To enable memory-mapped file support, call `ironclad.patch_native_filenos()`; this will patch `file` and `open`, and several functions on the `os` module, such that IronPython uses the CPython file type instead of its own.

See the wiki FrontPage for further instructions.

## Progress ##

The latest release is v2.6.0rc1, which should now be useful to quite a lot of people. The following packages have been confirmed to work (to a greater or lesser extent, as qualified parenthetically):

  * `numpy` 1.3.0 (over 1500 tests pass; Unicode data and `numpy.distutils` don't work).
  * `numpy` 1.4.0rc1 (over 1900 tests pass; similar issues to 1.3.0).
  * `scipy` 0.7.1 (over 2300 tests pass; a few parts are still rather slow)
  * `bz2` from Python 2.6 (well tested)
  * `_csv` from Python 2.6 (a couple of issues remain)

Lots of other packages work reasonably well; try them and see. Regrettably, this release does not support `PIL` (IronPython bug) or `matplotlib` (blame unclear). While `h5py` will run, it suffers from serious threading issues, and I can't recommend it for production use with IronPython. See the readme for more details.

## Other ##

Ironclad basically works by reimplementing the Python C API in C#, and performing a little bit of underhanded trickery to convince .pyd files to talk to our version of the API. The source distribution includes full tests and decent explanatory documentation.

Ironclad is fully open source, and is developed and supported by Resolver Systems and William Reade. He gratefully acknowledges Resolver Systems' generous and invaluable support, without which the project would never have got off the ground.

On that note, please download and play with Resolver One. It's a game-changing next-generation spreadsheet with full IronPython integration, it includes numpy support via Ironclad, and it is entirely a Good Thing.