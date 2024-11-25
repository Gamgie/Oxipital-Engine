
var numForceGroupsParam = local.parameters.setup.numForceGroups;
var numOrbGroupsParam = local.parameters.setup.numOrbGroups;
var numMacrosParam = local.parameters.setup.numMacros;

var macrosGroup = local.parameters.macros;
var forceGroupsGroup = local.parameters.forceGroups;
var orbGroupsGroup = local.parameters.orbGroups;

var forces = [];
var orbGroups = [];
var macros = [];

//Unity links
var unityBallet = null;
var unityForcesManager = null;
var unityOrbsManager = null;
var unityForceGroupsParam = null;
var unityOrbGroupsParam = null;

var lastSyncTime = 0;

var danceGroupParameters = {
	"Patterns": {
		"Count": { "type": "float", "default": 1, "min": 1, "max": 10, "noMacro": true },
		"Pattern Size": { "type": "float", "default": 1, "min": 0, "max": 20 },
		"Pattern Size Spread": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Pattern Axis Spread": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Line Pattern Weight": { "type": "float", "default": 0, "min": 0, "max": 1, "customComponent": "LineDancePattern/weight" },
		"Line Pattern Speed Weight": { "type": "float", "default": 0, "min": 0, "max": 1, "customComponent": "LineDancePattern/speedWeight" },
		"Circle Pattern Weight": { "type": "float", "default": 1, "min": 0, "max": 1, "customComponent": "CircleDancePattern/weight" },
		"Circle Pattern Speed Weight": { "type": "float", "default": 1, "min": 0, "max": 1, "customComponent": "CircleDancePattern/speedWeight" },
		"NBody Pattern Weight": { "type": "float", "default": 0, "min": 0, "max": 1, "customComponent": "NBodyProblemPattern/weight" },
		"NBody Pattern Speed Weight": { "type": "float", "default": 0, "min": 0, "max": 1, "customComponent": "NBodyProblemPattern/speedWeight" }
	},
	"Animation": {
		"Pattern Speed": { "type": "float", "default": 0.1, "min": -1, "max": 1 },
		"Pattern Speed Random": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Pattern Time Offset": { "type": "float", "default": 0, "min": 0, "max": 1 }

	},
	"Dancer": {
		"Dancer Size": { "type": "float", "default": 1, "min": 0, "max": 20 },
		"Dancer Hyper Size": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Dancer Size Spread": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Dancer Weight Size Factor": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Dancer Intensity": { "type": "float", "default": 1, "min": 0, "max": 1 },
		"Dancer Weight Intensity Factor": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Dancer Rotation": { "type": "p3d", "default": [0, 0, 0] },
		"Dancer Look At": { "type": "p3d", "default": [0, 1, 0] },
		"Dancer Look At Mode": { "type": "float", "default": 0, "min": 0, "max": 2 }
	}
};

var forceGroupParameters = {
	"General": {
		"Force Factor Inside": { "type": "float", "default": 1, "min": 0, "max": 1 },
		"Force Factor Outside": { "type": "float", "default": 0, "min": 0, "max": 1 }
	},
	"Radial": {
		"Radial Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Radial Frequency": { "type": "float", "default": 0, "min": 0, "max": 1 }
	},
	"Axial": {
		"Axial Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Axis Multiplier": { "type": "p3d", "default": [0, 1, 0] },
		"Axial Factor": { "type": "float", "default": 1, "min": 1, "max": 3 },
		"Axial Frequency": { "type": "p3d", "default": [0, 0, 0] }
	},
	"Linear": {
		"Linear Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 }
	},
	"Orthoradial": {
		"Ortho Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Ortho Inner Radius": { "type": "float", "default": 0.5, "min": 0, "max": 1 },
		"Ortho Factor": { "type": "float", "default": 2, "min": 1, "max": 3 },
		"Ortho Clockwise": { "type": "float", "default": 1, "min": -1, "max": 1 }
	},
	"Turbulence Curl": {
		"Curl Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Curl Frequency": { "type": "float", "default": 0, "min": 0, "max": 5 },
		"Curl Drag": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Curl Octaves": { "type": "float", "default": 1, "min": 1, "max": 8 },
		"Curl Roughness": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Curl Lacunarity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Curl Scale": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Curl Translation": { "type": "float", "default": 0, "min": 0, "max": 1 }
	},
	"Perlin": {
		"Perlin Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Perlin Frequency": { "type": "float", "default": 0, "min": 0, "max": 5 },
		"Perlin Octaves": { "type": "float", "default": 1, "min": 1, "max": 8 },
		"Perlin Roughness": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Perlin Lacunarity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Perlin Translation Speed": { "type": "float", "default": 0, "min": 0, "max": 1 }
	},
	"Orthoaxial": {
		"Orthoaxial Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Orthoaxial Inner Radius": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Orthoaxial Factor": { "type": "float", "default": 1, "min": 1, "max": 3 },
		"Orthoaxial Clockwise": { "type": "float", "default": 0, "min": 0, "max": 1 }
	}
};


var orbGroupParameters = {
	"General": {
		"Life": { "type": "float", "default": 20, "min": 0, "max": 40 },
		"Emitter Shape": { "type": "enum", "default": "Sphere", "values": ["Sphere", "Plane", "Torus", "Cube", "Pipe", "Egg", "Line", "Circle", "Merkaba", "Pyramid", "Custom", "Augmenta"] },
		"Emitter Surface Factor": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Emitter Volume Factor": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Emitter Position Noise": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Emitter Position Noise Frequency": { "type": "float", "default": 1, "min": 0, "max": 5 },
		"Emitter Position Noise Radius": { "type": "float", "default": 0.1, "min": 0, "max": 1 },
		
	},
	"Appearance": {
		"Color": { "type": "color", "default": [.8, 2, .05] },
		"Alpha": { "type": "float", "default": 0.2, "min": 0, "max": 1 },
		"HDR Multiplier": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Alpha Speed Threshold": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Texture Opacity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Particle Size": { "type": "float", "default": 0, "min": 0, "max": 1 },
	},
	"Physics": {
		"Force Weight": { "type": "float", "default": 1, "min": 0, "max": 1 },
		"Drag": { "type": "float", "default": 0.5, "min": 0, "max": 1 },
		"Velocity Drag": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Noisy Drag": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Noisy Drag Frequency": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Activate Collision": { "type": "bool", "default": false }
	}
};


//CALLBACKS
function init() {
	linkUnity();
	setup();
}


function moduleParameterChanged(param) {

	//Auto trigger sync when connected, and some timing safety because triggering sync data causes the module to disconnect / reconnect
	if(param.is(local.parameters.syncData))
	{
		lastSyncTime = util.getTime();
	}else if(param.is(local.parameters.isConnected))
	{
		if(local.parameters.isConnected.get())
		{
			if(util.getTime() > lastSyncTime + 30)
			{
				local.parameters.syncData.trigger();
			}
		}
	}else if (param.is(numForceGroupsParam)) {
		if (unityForceGroupsParam) unityForceGroupsParam.set(numForceGroupsParam.get());
		setupForces();
		linkArrays();
	} else if (param.is(numOrbGroupsParam)) {
		if (unityOrbGroupsParam) unityOrbGroupsParam.set(numOrbGroupsParam.get());
		setupOrbs();
		linkArrays();
	} else if (param.is(numMacrosParam)) {
		setupMacros();
	} else if (param.getParent().is(macrosGroup)) {
		updateAllParametersForMacro(param);
	} else {
		var p4 = param.getParent(4);
		if (p4 == forceGroupsGroup) {
			var forceIndex = parseInt(param.getParent(3).niceName.split(" ")[2]) - 1;
			var groupName = param.getParent(2).niceName;
			var paramName = param.getParent().niceName;
			updateParam(forceIndex, groupName, paramName, param, forceGroupParameters, forces);
		} else if (p4 == orbGroupsGroup) {
			var orbGroupIndex = parseInt(param.getParent(3).niceName.split(" ")[2]) - 1;
			var groupName = param.getParent(2).niceName;
			var paramName = param.getParent().niceName;
			updateParam(orbGroupIndex, groupName, paramName, param, orbGroupParameters, orbGroups);
		}
	}
}

function dataStructureEvent() {
	linkUnity();
}


//UPDATE
function updateAllParametersForMacro(macroParam) {
	var index = macrosGroup.getControllables().indexOf(macroParam);

	updateAllParameters(index, forces, forceGroupParameters);
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

function updateParam(index, groupName, paramName, sourceParam, parameters, items) {
	if (sourceParam != null) {
		var macroIndex = sourceParam.getParent().getControllables().indexOf(sourceParam);
		if (macroIndex > 0 && macros[macroIndex - 1].get() == 0) {
			return;
		}
	}

	var targetParams = danceGroupParameters[groupName] ? danceGroupParameters : parameters;

	var paramProps = targetParams[groupName][paramName];

	// script.log("Updating " + groupName + " " + paramName + " for " + items + "," + index);

	var item = items[index];
	var itemGroup = item.getChild(groupName);
	var itemParamGroup = itemGroup.getChild(paramName);
	var itemParam = itemParamGroup.getChild("baseValue");
	var paramMin = paramProps.min;
	var paramMax = paramProps.max;

	var managerName = item.getParent().niceName;

	var finalValue = itemParam.get();

	if (!paramProps.noMacro) {
		if (paramMin != null && paramMax != null) {
			for (var i = 0; i < numMacrosParam.get(); i++) {
				var macroWeight = itemParamGroup.getChild("macroWeight" + (i + 1)).get();
				var macroValue = macros[i].get();
				var macroInfluence = macroValue * macroWeight * (paramMax - paramMin);
				finalValue += macroInfluence;
			}
		}
	}

	if (paramMin != null && paramMax != null) finalValue = Math.min(paramMax, Math.max(paramMin, finalValue));

	
	var paramPath = "";
	if(paramProps.customComponent != null) {
		// script.log("Using custom component : " + paramProps.customComponent);
		paramPath = paramProps.customComponent;
	}else{
		var unityComponentName = parameters == forceGroupParameters ? "StandardForceGroup" : "OrbGroup";
		paramPath = unityComponentName + "/" + itemParamGroup.name;
	}


	updateUnityParam(managerName, item.niceName, paramPath, finalValue);
}

function updateUnityParam(managerName, itemName, paramPath, value) {
	if (unityBallet == null) return;

	var manager = unityBallet.getChild(managerName);
	if (manager == null) return;

	var item = manager.getChild(itemName);
	if (item == null) return;

	var param = item.getChild(paramPath);
	if (param == null) return;

	if (value != null) param.set(value);
}

// SETUP

function setup() {
	setupMacros();
	setupForces();
	setupOrbs();

	linkArrays();
}

function clearItems(group) {
	group.clear();
}

function linkArrays() {
	macros = macrosGroup.getControllables();
	forces = forceGroupsGroup.getContainers();
	orbGroups = orbGroupsGroup.getContainers();
}

function linkUnity() {
	unityBallet = local.values.getChild("ballet");
	if (unityBallet == null) {
		script.log("No ballet found");
		unityOrbs = null;
		unityForces = null;
		unityOrbGroupsParam = null;
		unityForceGroupsParam = null;
		return;
	}

	unityOrbs = unityBallet.getChild(orbGroupsGroup.niceName);
	unityForces = unityBallet.getChild(forceGroupsGroup.niceName);

	unityOrbGroupsParam = unityOrbs.OrbManager.count;
	unityForceGroupsParam = unityForces.StandardForceManager.count;

	if (unityOrbGroupsParam) unityOrbGroupsParam.set(numOrbGroupsParam.get());
	if (unityForceGroupsParam) unityForceGroupsParam.set(numForceGroupsParam.get());
}

function setupMacros() {
	if (macrosGroup.getControllables().length == numMacrosParam.get()) return;

	while (macrosGroup.getControllables().length > numMacrosParam.get()) {
		macrosGroup.removeParameter("macro" + (macrosGroup.getControllables().length));
	}

	while (macrosGroup.getControllables().length < numMacrosParam.get()) {
		var macro = macrosGroup.addFloatParameter("Macro " + (macrosGroup.getControllables().length + 1), "Macro value", 0, 0, 1);
	}

	macrosGroup = local.parameters.getChild("macros");
	macros = macrosGroup.getControllables();

	setupForces();
	setupOrbs();

	linkArrays();
}

function setupForces() {
	if (numForceGroupsParam == null) return;
	setupParameters(forceGroupsGroup, numForceGroupsParam, forceGroupParameters, forces, "Force Group");
}

function setupOrbs() {
	if (numOrbGroupsParam == null) return;
	setupParameters(orbGroupsGroup, numOrbGroupsParam, orbGroupParameters, orbGroups, "Orb Group");
}

function setupParameters(group, numParam, parameters, items, prefix) {

	var numItems = numParam.get();

	var existingItems = group.getContainers();
	var numExistingItems = existingItems ? existingItems.length : 0;

	while (numExistingItems > numItems) {
		group.removeContainer(existingItems[numExistingItems - 1].name);
		numExistingItems--;
		items.splice(items.length - 1);
	}

	for (var i = 0; i < Math.min(numExistingItems, numItems); i++) {
		setupMacrosToItem(existingItems[i]);
	}

	for (var i = numExistingItems; i < numItems; i++) {
		createItem(i, group, parameters, prefix);
	}

	items = group.getContainers();
}

function createItem(index, group, parameters, prefix) {
	var item = group.addContainer(prefix + " " + (index + 1));
	item.setCollapsed(true);

	addParametersToItem(item, danceGroupParameters);
	addParametersToItem(item, parameters);

	setupMacrosToItem(item);

	return item;
}

function addParametersToItem(item, parameters) {
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
				var ep = paramContainer.addEnumParameter("Base Value", "Base value for this parameter", paramDefault);
				for (var v = 0; v < paramConfig.values.length; v++) {
					ep.addOption(paramConfig.values[v], paramConfig.values[v]);
				}
			} else if (paramType == "bool") {
				paramContainer.addBoolParameter("Base Value", "Base value for this parameter", paramDefault);
			}
		}
	}
}

function setupMacrosToItem(item) {
	var paramGroups = item.getContainers();
	for (var i = 0; i < paramGroups.length; i++) {
		var paramGroup = paramGroups[i];
		// script.log('> ' + paramGroup.niceName);;
		var params = paramGroup.getContainers();
		for (var j = 0; j < params.length; j++) {
			var paramContainer = params[j];
			// script.log(param);
			var paramCurrentMacros = paramContainer.getControllables().length - 1; //first is base value
			var numMacros = numMacrosParam.get();

			var paramProp = getPropForParam(paramContainer);
			if(paramProp.noMacro == true) numMacros = 0;

			for (var k = paramCurrentMacros; k > numMacros; k--) paramContainer.removeParameter("Macro Weight " + k);

			for (var k = paramCurrentMacros; k < numMacros; k++) {
				if (paramProp.noMacro == true) continue;
				if (paramProp.type == "float" || paramProp.type == "int" && (paramProp.min != null && paramProp.min != null)) {
					for (var k = 0; k < numMacrosParam.get(); k++) {
						paramContainer.addFloatParameter("Macro Weight " + (k + 1), "Macro influence for this parameter, relative to the parameters range if it has any", 0, -1, 1);
					}
				}
			}
		}
	}
}

function getPropForParam(param) {
	var paramName = param.niceName;
	var paramGroupName = param.getParent().niceName;

	var group = param.getParent(3);

	var isDanceGroup = danceGroupParameters[paramGroupName] != null;
	var items = isDanceGroup ? danceGroupParameters : (group.is(forceGroupsGroup) ? forceGroupParameters : orbGroupParameters);

	return items[paramGroupName][paramName];
}