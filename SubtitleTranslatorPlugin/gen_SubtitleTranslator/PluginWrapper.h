#pragma once

ref class PluginWrapper
{
public:
	// The plugin itself
	static SubtitleTranslator::GeneralPlugin^ plugin = gcnew SubtitleTranslator::ThePlugin();
	// Name of the plugin
	static char * Name();
};

