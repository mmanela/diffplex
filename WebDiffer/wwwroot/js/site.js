

function InitializeDiffPanes() {
    var diffBox = $("#diffBox");
    var parent = diffBox.parent();
    var diffPane = $(".diffPane", diffBox);
    var leftTable = $(".diffTable", diffPane[0]);
    var rightTable = $(".diffTable", diffPane[1]);
    var diffPaneLinesLeft = $("td.line", leftTable);
    var diffPaneLineHeight = diffPaneLinesLeft.outerHeight();
    var scrollBarsActive = false;

    SizeDiffTablesEqually();
    SizeDiffPanesToWindow();


    // synchronize the scroll bars
    $(diffPane[0]).scroll(OnLeftDiffPaneScroll);
    $(diffPane[1]).scroll(OnRightDiffPaneScroll);


    $(window).resizeComplete(function () {
        SizeDiffTablesEqually();
        SizeDiffPanesToWindow();

    });


    function SizeDiffPanesToWindow() {
        var lineCount = diffPaneLinesLeft.length;
        var contentHeight = lineCount * diffPaneLineHeight;

        diffPane.hide();
        var parentHeight = parent.outerHeight(true);
        var parentTop = parent.offset().top;
        var windowHeight = $(window).height();
        var newHeight = windowHeight - (parentHeight + parentTop);
        diffPane.show();

        if (contentHeight < newHeight) {
            newHeight = contentHeight;
            if (scrollBarsActive)
                newHeight += diffPaneLineHeight + 3;
        }

        if (newHeight > 0)
            diffPane.height(newHeight);

    }

    function SizeDiffTablesEqually() {
        var maxWidth = Math.max(leftTable.width(), rightTable.width());
        var maxHeight = Math.max(leftTable.height(), rightTable.height());


        leftTable.height(maxHeight);
        rightTable.height(maxHeight);

        if (diffPane.width() < maxWidth) {
            leftTable.width(maxWidth);
            rightTable.width(maxWidth);
            scrollBarsActive = true;
        }
    }

    function OnLeftDiffPaneScroll(e) {
        var left = this.scrollLeft;
        var top = this.scrollTop;
        if (top != diffPane[1].scrollTop) diffPane[1].scrollTop = top;
        if (left != diffPane[1].scrollLeft) diffPane[1].scrollLeft = left;
    }

    function OnRightDiffPaneScroll(e) {
        var left = this.scrollLeft;
        var top = this.scrollTop;
        if (top != diffPane[0].scrollTop) diffPane[0].scrollTop = top;
        if (left != diffPane[0].scrollLeft) diffPane[0].scrollLeft = left;
    }
}


jQuery.fn.resizeComplete = function (callback) {

    var element = this;
    var height = element.height();
    var width = element.width();
    var monitoring = false;
    var timer;

    function monitorResizing() {

        if (!same()) {
            height = element.height();
            width = element.width();
            timer = setTimeout(function () { monitorResizing() }, 500);
        }
        else {
            clearTimeout(timer);
            callback();
            monitoring = false;
        }
    }

    function same() {
        var newHeight = element.height();
        var newWidth = element.width();

        return newHeight == height && newWidth == width;
    }

    function onResize() {
        if (monitoring) return;
        if (same()) return;
        monitoring = true;
        monitorResizing();
    }

    element.resize(onResize);
 
}