/*
  This code is part of the Niffty NIFF Music File Viewer. 
  It is distributed under the GNU Public Licence (GPL) version 2.  See
  http://www.gnu.org/ for further details of the GPL.
 */

using System;
namespace org.niffty {

  /** A RiffPageHeader provides static methods for encoding/decoding a
   * PageHeader using RIFF.
   *
   * @author  default
   */
  public class RiffPageHeader {
    static readonly String RIFF_ID = "pghd";

    /** Suppress the default constructor.
     */
    private RiffPageHeader() {
    }

    /** Creates new PageHeader from the parentInput's input stream.
     * The next object in the input stream must be of this type.
     *
     * @param parentInput    the parent RIFF object being used to read the input stream
     */
    static public PageHeader newInstance(Riff parentInput) {
      Riff riffInput = new Riff(parentInput, RIFF_ID);

      // empty required part
      return new PageHeader(RiffTags.newInstance(riffInput));
    }
  }
}