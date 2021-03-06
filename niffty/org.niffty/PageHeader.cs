/*
  This code is part of the Niffty NIFF Music File Viewer. 
  It is distributed under the GNU Public Licence (GPL) version 2.  See
  http://www.gnu.org/ for further details of the GPL.
 */

using System;
namespace org.niffty {

  /** A PageHeader has optional tags: width and height.
   *
   * @author  user
   */
  public class PageHeader {
    /** Creates a new PageHeader with the given tags.
     *
     * @param tags  the tags for this page header.  If this is null,
     *          then this uses an empty Tags object.
     */
    public PageHeader(Tags tags) {
      if (tags == null)
        tags = new Tags();

      _tags = tags;
    }

    /** Returns the Tags object containing the optional tags.
     */
    public Tags getTags() {
      return _tags;
    }

    public override string ToString() {
      return "Page-header" + _tags;
    }

    private Tags _tags;
  }
}