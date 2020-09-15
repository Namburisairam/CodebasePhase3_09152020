(function (eventFormWaiverValues, eventID) {
    "use strict";
    $(document).ready(function () {

        var container = $('.form-waiver-config-element');

        function createNewMappingElement(eventFormWaiverKeys, mapping) {
            var mappingContainer = $('<tr>');
            var pdfElemntsDrpDwn = generateDropdown(_.map(eventFormWaiverKeys, function (data) {
                return { id: data.ID, value: data.FormKey };
            }), mapping.ID);
            var drpDownContainer = $('<td>').append(pdfElemntsDrpDwn);

            mappingContainer.append(drpDownContainer);
            var pdfElemntValuesDrpDwn = generateDropdown(_.map(eventFormWaiverValues, function (data) {
                return { id: data.ID, value: data.Name };
            }), mapping.Value);

            var drpDownContainer = $('<td>').append(pdfElemntValuesDrpDwn);
            mappingContainer.append(drpDownContainer);
            //var deleteBtn = $('<button type="button" class="btn btn-danger" >').text("Delete").click(deleteMapping);
            //mappingContainer.append($('<td>').append(deleteBtn));
            container.append(mappingContainer);
        }

        function generateDropdown(dataArray, value) {
            var drpDwn = $('<select class= "form-control" />').css("width", "200px").css("margin", "auto");
            $.each(dataArray, function (i, el) {
                var option = $("<option></option>").val(el.id).html(el.value);
                drpDwn.append(option);
            });
            drpDwn.val(value);
            return drpDwn;
        }

        // Default element
        //createNewMappingElement();

        $('#addNewWaiverMap').click(addNewMappingElement);

        function addNewMappingElement() {
            createNewMappingElement();
        }

        function deleteMapping() {
            var parentDom = $(this).closest('tr');
            parentDom.remove();
        }

        function getMappings() {
            var mappingData = [];
            $('#waiverMappings tr').each(function () {
                var tableData = $(this).find('td');
                if (tableData.length > 0) {
                    var dictObj = {};
                    var mappingKey = $(tableData[0].firstElementChild).val();
                    dictObj.Key = mappingKey;
                    // Need to use ES5 instead of ES6
                    var value = +$(tableData[1].firstElementChild).val();
                    dictObj.Value = value;
                    mappingData.push(dictObj);
                }
            });
            return mappingData;
        }

        $('#saveMappings').click(function () {
            var mappingData = getMappings();
            var activityID = $('#formModalActivityID').val();
            $.ajax({
                type: 'POST',
                url: '/events/SaveWaiverFormMappingData',
                dataType: 'json',
                data: { mappingData: mappingData, EventID: eventID, activityID: activityID },
                success: function (data) {
                    $('#waiverFormModal').modal('toggle');
                }
            });
        });

        $('.configureWaiverForm').click(function () {
            var activityID = $(this).attr('data-activityID');
            container.empty();
            $('#formModalActivityID').val(activityID);
            $.ajax({
                type: 'POST',
                url: '/events/GetPDFConfig',
                dataType: 'json',
                data: { activityID: activityID },
                success: function (waiverFormTemplateMappings) {

                    var configValues = _.filter(waiverFormTemplateMappings, function (data) {
                        return data.Value;
                    });

                    configValues = _.size(configValues) ? configValues : [{}];

                    _.forEach(configValues, function (mapping) {
                        createNewMappingElement(waiverFormTemplateMappings, mapping);
                    });
                }
            });
        });
    });
})(eventFormWaiverValues, 33);