var numForcesParam = local.parameters.setup.numForces;
var numOrbGroupsParam = local.parameters.setup.numOrbGroups;
var numMacrosParam = local.parameters.setup.numMacros;

var macrosGroup = local.parameters.macros;
var forcesGroup = local.parameters.forces;
var orbGroupsGroup = local.parameters.orbGroups;

var forces = [];
var orbGroups = [];
var macros = [];

//Unity links
var unityForcesGroup = local.values.orbs.forces;

var forceParameters = {
	"General": {
		"Position": { "type": "p3d", "default": [0, 0, 0] },
		"Radius": { "type": "float", "default": 0, "min": 0, "max": 15 },
		"Intensity": { "type": "float", "default": 0, "min": 0, "max": 100 },
	},
	"Turbulence": {
		"Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Frequency": { "type": "float", "default": 0, "min": 0 },
		"Octaves": { "type": "int", "default": 0, "min": 0, "max": 6 },
	},
	"Vortex": {
		"Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Frequency": { "type": "float", "default": 0, "min": 0, "max": 20 },
	},
	"Gravity": {
		"Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Axis": { "type": "p3d", "default": [0, 1, 0] },
	}
};

var orbGroupParameters = {
	"General": {
		"Position": { "type": "p3d", "default": [0, 0, 0] },
		"Rotation": { "type": "p3d", "default": [0, 0, 0] },
		"Scale": { "type": "p3d", "default": [1, 1, 1] },
		"Size": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Size Offset": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Life": { "type": "float", "default": 0, "min": 0, "max": 200 },
	},
	"Emission": {
		"Rate": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Shape": { "type": "enum", "default": "Sphere", "values": ["Sphere", "Plane", "Torus", "Cube", "Pipe", "Egg", "Line", "Circle", "Merkaba", "Pyramid", "Landscape"] },
		"Mode": { "type": "enum", "default": "Surface", "values": ["Surface", "Edge"] },
		"Volumize": { "type": "float", "default": 0, "min": 0, "max": 1 },
	},
	"Appearance": {
		"Size": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Alpha": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Color": { "type": "color", "default": [1, 1, 1] },
		"Color Multiplier": { "type": "float", "default": 0, "min": 0, "max": 5 },
		"Stationary": { "type": "float", "default": 0, "min": 0, "max": 1 }
	},
	"Physics": {
		"Forces Multiplier": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Drag": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Velocity Drag": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Noisy Drag": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Noisy Drag Frequency": { "type": "float", "default": 0, "min": 0, "max": 1 }
	}
};

//CALLBACKS
function init() {
	setupMacros();
	setupForces();
	setupOrbs();

	linkArrays();
}

function moduleParameterChanged(param) {
	if (param.is(numForcesParam)) {
		setupForces();
	} else if (param.is(numOrbGroupsParam)) {
		setupOrbs();
	} else if (param.is(numMacrosParam)) {
		setupMacros();
	} else if (param.getParent().is(macrosGroup)) {
		updateAllParametersForMacro(param);
	} else {
		var p4 = param.getParent(4);
		if (p4 == forcesGroup) {
			var forceIndex = parseInt(param.getParent(3).niceName.split(" ")[1]) - 1;
			var groupName = param.getParent(2).niceName;
			var paramName = param.getParent().niceName;
			updateParam(forceIndex, groupName, paramName, param, forceParameters, forces);
		} else if (p4 == orbGroupsGroup) {
			var orbGroupIndex = parseInt(param.getParent(3).niceName.split(" ")[2]) - 1;
			var groupName = param.getParent(2).niceName;
			var paramName = param.getParent().niceName;
			updateParam(orbGroupIndex, groupName, paramName, param, orbGroupParameters, orbGroups);
		}
	}
}

//UPDATE
function updateParam(index, groupName, paramName, sourceParam, parameters, items) {
	if (sourceParam != null) {
		var macroIndex = sourceParam.getParent().getControllables().indexOf(sourceParam);
		if (macroIndex > 0 && macros[macroIndex - 1].get() == 0) {
			return;
		}
	}

	var item = items[index];
	var itemGroup = item.getChild(groupName);
	var itemParamGroup = itemGroup.getChild(paramName);
	var itemParam = itemParamGroup.getChild("baseValue");
	var paramMin = parameters[groupName][paramName].min;
	var paramMax = parameters[groupName][paramName].max;

	var finalValue = itemParam.get();

	if (paramMin != null && paramMax != null) {
		for (var i = 0; i < numMacrosParam.get(); i++) {
			var macroWeight = itemParamGroup.getChild("macroWeight" + (i + 1)).get();
			var macroValue = macros[i].get();
			var macroInfluence = macroValue * macroWeight * (paramMax - paramMin);
			finalValue += macroInfluence;
		}
	}

	finalValue = Math.min(paramMax, Math.max(paramMin, finalValue));

	script.log("Update Param " + (index + 1) + " " + groupName + " " + paramName + " " + finalValue);
}

function updateAllParametersForMacro(macroParam) {
	var index = macrosGroup.getControllables().indexOf(macroParam);

	updateAllParameters(index, forces, forceParameters);
	updateAllParameters(index, orbGroups, orbGroupParameters);
}

function updateAllParameters(index, items, parameters) {
	for (var i = 0; i < items.length; i++) {
		var item = items[i];
		var itemGroups = item.getContainers();
		for (var j = 0; j < itemGroups.length; j++) {
			var itemGroup = itemGroups[j];
			var itemParams = itemGroup.getContainers();
			for (var k = 0; k < itemParams.length; k++) {
				var itemParamGroup = itemParams[k];

				var itemParamChildren = itemParamGroup.getControllables();
				if (itemParamChildren.length <= index + 1) continue;
				var macroParam = itemParamChildren[index + 1];
				if (macroParam.get() == 0) continue;
				updateParam(i, itemGroup.niceName, itemParamGroup.niceName, macroParam, parameters, items);
			}
		}
	}
}

// SETUP
function clearItems(group) {
	group.clear();
}

function linkArrays() {
	macros = macrosGroup.getControllables();
	forces = forcesGroup.getContainers();
	orbGroups = orbGroupsGroup.getContainers();
}

function setupMacros() {
	if (macrosGroup.getControllables().length == numMacrosParam.get()) return;

	while (macrosGroup.getControllables().length > numMacrosParam.get()) {
		macrosGroup.removeParameter("macro" + (macrosGroup.getControllables().length));
	}

	while (macrosGroup.getControllables().length < numMacrosParam.get()) {
		var macro = macrosGroup.addFloatParameter("Macro " + (macrosGroup.getControllables().length + 1), "Macro value", 0, 0, 1);
	}

	macros = macrosGroup.getControllables();
	setupForces();
	setupOrbs();
}

function setupForces() {
	setupParameters(local.parameters.setup.clearOnSetup.get(), forcesGroup, numForcesParam, forceParameters, forces, "Force");
}

function setupOrbs() {
	setupParameters(local.parameters.setup.clearOnSetup.get(), orbGroupsGroup, numOrbGroupsParam, orbGroupParameters, orbGroups, "Orb Group");
}

function setupParameters(clear, group, numParam, parameters, items, prefix) {
	if (clear) clearItems(group);

	var numItems = numParam.get();

	var existingItems = group.getContainers();
	var numExistingItems = existingItems ? existingItems.length : 0;

	while (numExistingItems > numItems) {
		group.removeContainer(existingItems[numExistingItems - 1].name);
		numExistingItems--;
		items.splice(items.length - 1);
	}

	for (var i = numExistingItems; i < numItems; i++) {
		createItem(i, group, parameters, prefix);
	}

	items = group.getContainers();
}

function createItem(index, group, parameters, prefix) {
	var item = group.addContainer(prefix +" "+ (index + 1));
	item.setCollapsed(true);
	var paramGroupProps = util.getObjectProperties(parameters);

	for (var i = 0; i < paramGroupProps.length; i++) {
		var groupName = paramGroupProps[i];
		var groupParams = parameters[groupName];

		var paramGroup = item.addContainer(groupName);
		var paramProps = util.getObjectProperties(groupParams);
		for (var j = 0; j < paramProps.length; j++) {
			var paramName = paramProps[j];
			var paramConfig = groupParams[paramName];
			var paramType = paramConfig.type;
			var paramDefault = paramConfig.default;
			var paramMin = paramConfig.min;
			var paramMax = paramConfig.max;

			var paramContainer = paramGroup.addContainer(paramName);

			if (paramType == "float") {
				if (paramMin != null && paramMax != null) paramContainer.addFloatParameter("Base Value", "Base value for this parameter", paramDefault, paramMin, paramMax);
				else if (paramMin != null) paramContainer.addFloatParameter("Base Value", "Base value for this parameter", paramDefault, paramMin);
				else paramContainer.addFloatParameter("Base Value", "Base value for this parameter", paramDefault);
			} else if (paramType == "int") {
				if (paramMin != null && paramMax != null) paramContainer.addIntParameter("Base Value", "Base value for this parameter", paramDefault, paramMin, paramMax);
				else if (paramMin != null) paramContainer.addIntParameter("Base Value", "Base value for this parameter", paramDefault, paramMin);
				else paramContainer.addIntParameter("Base Value", "Base value for this parameter", paramDefault);
			} else if (paramType == "p3d") {
				paramContainer.addPoint3DParameter("Base Value", "Base value for this parameter", paramDefault);
			} else if (paramType == "color") {
				paramContainer.addColorParameter("Base Value", "Base value for this parameter", paramDefault);
			} else if (paramType == "enum") {
				paramContainer.addEnumParameter("Base Value", "Base value for this parameter", paramDefault, paramConfig.values);
			}

			if (paramType == "float" || paramType == "int" && (paramMin != null && paramMax != null)) {
				for (var k = 0; k < numMacrosParam.get(); k++) {
					paramContainer.addFloatParameter("Macro Weight " + (k + 1), "Macro influence for this parameter, relative to the parameters range if it has any", 0, -1, 1);
				}
			}
		}
	}

	return item;
}
